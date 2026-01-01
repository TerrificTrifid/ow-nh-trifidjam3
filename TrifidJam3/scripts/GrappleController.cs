using Mono.Cecil;
using NewHorizons;
using NewHorizons.Components;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3;

[UsedInUnityProject]
public class GrappleController : OWItem
{
	public static GrappleController Instance { get; private set; }

	public static float MaxLength = 75f;
	public static float MinLength = 2f;
	public static float ReelMaxSpeed = 40f;
	public static float ReelInitialSpeed = 10f;
	public static float ReelAcceleration = 80f;
	public static float SpringStrength = 0.2f;
	public static float SpringDamper = 0.01f;
	public static float Spring2Strength = 0.02f;
	public static float Spring2Damper = 0.04f;

	public IInputCommands ActivateKey = InputLibrary.toolActionPrimary;
    public IInputCommands ReelInKey = InputLibrary.toolOptionUp;
    public IInputCommands ReelOutKey = InputLibrary.toolOptionDown;
    private ScreenPrompt _activatePrompt;
	private ScreenPrompt _reelInPrompt;
	private ScreenPrompt _reelOutPrompt;

	public Light Light;
	public float LightActiveIntensity = 0.8f;
	public float LightActiveRange = 8f;
	public float LightInactiveIntensity = 0.4f;
	public float LightInactiveRange = 8f;
	public Transform AttachPoint;
	public GrappleEndpoint Endpoint;
	public GameObject ChargeEffect;

	public AudioClip AmbienceAudio;
	private OWAudioSource _ambienceAudioSource;
	public AudioClip ReelAudio;
    private OWAudioSource _reelAudioSource;
    public AudioClip ActivateAudio;
	public AudioClip ReleaseAudio;
	public AudioClip ChargeAudio;
	public AudioClip DischargeAudio;
	private OWAudioSource _oneShotAudioSource;

	private bool _grappleActive;
	private bool _grappleConnected;
	private int _reelDirection;
	private float _reelSpeed;
	private float _targetLength;
	private SpringJoint _joint;
	private SpringJoint _joint2;
	private bool _charged;

	public override void Awake()
	{
		Instance = this;
		_type = ItemType.VisionTorch;
		base.Awake();
	}

	private void Start()
	{
		_activatePrompt = new ScreenPrompt(ActivateKey, TranslationHandler.GetTranslation("Grapple_Activate", TranslationHandler.TextType.UI) + "   <CMD>");
		_reelInPrompt = new ScreenPrompt(ReelInKey, TranslationHandler.GetTranslation("Grapple_ReelIn", TranslationHandler.TextType.UI) + "   <CMD>");
		_reelOutPrompt = new ScreenPrompt(ReelOutKey, TranslationHandler.GetTranslation("Grapple_ReelOut", TranslationHandler.TextType.UI) + "   <CMD>");

		//Endpoint.gameObject.SetActive(false);

		var playerAudioController = Locator.GetPlayerAudioController();

		_ambienceAudioSource = Instantiate(
			playerAudioController._oneShotSource,
			playerAudioController._oneShotSource.transform.parent
		);
        _reelAudioSource = Instantiate(
            playerAudioController._oneShotSource,
            playerAudioController._oneShotSource.transform.parent
        );
        _oneShotAudioSource = Instantiate(
			playerAudioController._oneShotSource,
			playerAudioController._oneShotSource.transform.parent
		);

        _ambienceAudioSource.clip = AmbienceAudio;
        _ambienceAudioSource.loop = true;
        _ambienceAudioSource.SetMaxVolume(0.3f);
        _reelAudioSource.clip = ReelAudio;
        _reelAudioSource.loop = true;
        _reelAudioSource.SetMaxVolume(0.5f);

        enabled = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Destroy(_ambienceAudioSource.gameObject);
        Destroy(_reelAudioSource.gameObject);
        Destroy(_oneShotAudioSource.gameObject);
		
	}

	public override string GetDisplayName()
	{
		return TranslationHandler.GetTranslation("Grapple_Name", TranslationHandler.TextType.UI);
	}

	public override void PickUpItem(Transform holdTranform)
	{
		base.PickUpItem(holdTranform);

		Locator.GetPromptManager().AddScreenPrompt(_activatePrompt, PromptPosition.UpperRight, true);
		Locator.GetPromptManager().AddScreenPrompt(_reelInPrompt, PromptPosition.UpperRight, true);
		Locator.GetPromptManager().AddScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight, true);

		enabled = true;
	}

	public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		base.DropItem(position, normal, parent, sector, customDropTarget);
			
		Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().RemoveScreenPrompt(_reelInPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().RemoveScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight);

		enabled = false;
	}

	public override void SocketItem(Transform socketTransform, Sector sector)
	{
		base.SocketItem(socketTransform, sector);
			
		Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().RemoveScreenPrompt(_reelInPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().RemoveScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight);

		enabled = false;
	}

	public override bool CheckIsDroppable()
	{
		return !_grappleActive;
	}

	/// <summary>
	/// Tells how much extra "rope" there is.
	/// </summary>
	public float GetTension()
	{
		Vector3 p1 = Locator.GetToolModeSwapper()._firstPersonManipulator.transform.position;
        Vector3 p2 = Endpoint.transform.position;
		return _targetLength - (p1 - p2).magnitude;
    }

	private void Update()
	{
		if (OWInput.IsPressed(ActivateKey, InputMode.Character))
		{
			/*if (TrifidJam3.Instance.Planet.transform.position.magnitude > 1000f)
            {
				NotificationManager.SharedInstance.PostNotification(new NotificationData(NotificationTarget.Player, TranslationHandler.GetTranslation("Grapple_OutOfRange", TranslationHandler.TextType.UI), 2f), false);
			}*/
			if (!_grappleActive)
			{
				//NHLogger.Log("activate");
				ActivateGrapple();
			}
		}
		else
		{
			if (_grappleActive)
			{
				//NHLogger.Log("release");
				DeactivateGrapple();
			}
        }

		if (_grappleConnected && OWInput.IsPressed(ReelInKey, InputMode.Character))
		{
			//NHLogger.Log("reel in");
			_reelDirection = -1;
		}
		else if (_grappleConnected && OWInput.IsPressed(ReelOutKey, InputMode.Character))
		{
			//NHLogger.Log("reel out");
			_reelDirection = 1;
		}
		else _reelDirection = 0;
	}

	private void FixedUpdate()
	{
		if (_grappleActive)
		{
			if (!_grappleConnected)
			{
				ConnectGrapple();
			}
			else
			{
                var player = Locator.GetPlayerBody();
				var camera = Locator.GetToolModeSwapper()._firstPersonManipulator.transform;

				if (_reelDirection != 0)
					_reelSpeed = Mathf.Clamp(_reelSpeed + ReelAcceleration * Time.fixedDeltaTime, ReelInitialSpeed, ReelMaxSpeed);
				else
					_reelSpeed = ReelInitialSpeed;
                _targetLength += _reelDirection * _reelSpeed * Time.fixedDeltaTime * (Mathf.Max(_targetLength, 20f) / MaxLength);
				_targetLength = Mathf.Clamp(_targetLength, MinLength - 1f, MaxLength - 1f);
				_joint.maxDistance = _targetLength;
				_joint.minDistance = _targetLength;

				var relativeEndpoint = camera.InverseTransformPoint(Endpoint.transform.position);
				var endpointDir = relativeEndpoint.normalized;
				var cameraDir = camera.forward;

				_joint2.connectedAnchor = player.transform.InverseTransformPoint(cameraDir * (relativeEndpoint.magnitude + 1f));

				//var torque = -(Vector3.Project(relativeEndpoint, cameraDir) * 2f - relativeEndpoint);
				/*var torqueDir = Vector3.Cross(endpointDir, Vector3.Cross(endpointDir, cameraDir));
				var torqueStrength = 1f - Vector3.Dot(endpointDir, cameraDir);
				if (torqueStrength > TorqueDeadzone)
				{
					var torqueForce = torqueDir * (torqueStrength - TorqueDeadzone) * TorqueStrength;
                    player.AddForce(torqueForce);
					var body = Endpoint.GetAttachedOWRigidbody();
					//var addPhysics = body.gameObject.GetComponent<AddPhysics>();
                    //if (addPhysics != null)
					//{
					//	body.UnsuspendImmediate(true);
					//	body.SetIsTargetable(true);
					//	Destroy(addPhysics);
					//}
					if (!body.IsKinematic())
					{
						body.AddForce(-torqueForce, Endpoint.transform.position);
					}
                }*/

				//var body = Endpoint.GetAttachedOWRigidbody().transform;
                //var anchor = body.InverseTransformPoint(Endpoint.transform.position);
				//var anchorShift = Vector3.Reflect(cameraDir, relativeEndpoint.normalized) * relativeEndpoint.magnitude;
				//_joint.anchor = body.InverseTransformPoint(player.transform.TransformPoint(anchorShift));


            }
		}
		else
		{
			if (_grappleConnected)
			{
				ReleaseGrapple();
			}
		}
	}

	public void ActivateGrapple()
	{
        Light.intensity = LightActiveIntensity;
        Light.range = LightActiveRange;

        _grappleActive = true;
    }

	public void DeactivateGrapple()
	{
        Light.intensity = LightInactiveIntensity;
        Light.range = LightInactiveRange;

		_grappleActive = false;
    }

	public void ConnectGrapple()
	{
		var aim = Locator.GetToolModeSwapper()._firstPersonManipulator.transform;
        if (Physics.Raycast(aim.position, aim.forward, out var hitInfo, MaxLength, OWLayerMask.groundMask))
        {
			if (hitInfo.distance < 1f || hitInfo.rigidbody.GetAttachedOWRigidbody() == null) return;
			//NHLogger.Log(hitInfo.rigidbody.gameObject.name);
			//NHLogger.Log(hitInfo.distance);
			//NHLogger.Log(hitInfo.point);

			Endpoint.transform.position = hitInfo.point;
            Endpoint.transform.LookAt(hitInfo.point + hitInfo.normal);
            Endpoint.transform.parent = hitInfo.collider.transform;
            Endpoint.gameObject.SetActive(true);
			Endpoint.UpdateLine();

			var player = Locator.GetPlayerBody();
			var playerPosition = player.transform.position;

			// need the initial spring length to be 1 lol
			player.MoveToPosition(playerPosition + (aim.forward * (hitInfo.distance - 1f)));
			//NHLogger.Log(hitInfo.point - Locator.GetActiveCamera().transform.forward);
			//NHLogger.Log(playerPosition + (Locator.GetActiveCamera().transform.forward * (hitInfo.distance - 1f)));

			_joint = hitInfo.rigidbody.gameObject.AddComponent<SpringJoint>();
			_joint.connectedBody = player.GetRigidbody();

            _joint2 = hitInfo.rigidbody.gameObject.AddComponent<SpringJoint>();
            _joint2.connectedBody = player.GetRigidbody();

            player.MoveToPosition(playerPosition);

			_joint.anchor = hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point);
			_joint.autoConfigureConnectedAnchor = false;
			_joint.connectedAnchor = Vector3.zero;
			_joint.enableCollision = true;
			_targetLength = hitInfo.distance; // - 1f
            _joint.maxDistance = _targetLength;
            _joint.minDistance = _targetLength;
			_joint.spring = SpringStrength;
			_joint.damper = SpringDamper;

			_joint2.anchor = Vector3.zero; //_joint.anchor;
            _joint2.autoConfigureConnectedAnchor = false;
            _joint2.connectedAnchor = player.transform.InverseTransformPoint(aim.forward * (hitInfo.distance + 1f));
            _joint2.enableCollision = true;
            _joint2.maxDistance = 0f;
            _joint2.minDistance = 0f;
			if (!hitInfo.rigidbody.isKinematic)
			{
				_joint2.spring = Spring2Strength;
				_joint2.damper = Spring2Damper;
			}
			else
			{
                _joint2.spring = 0;
                _joint2.damper = 0;
            }

            _oneShotAudioSource.PlayOneShot(ActivateAudio, 1f);
			_ambienceAudioSource.FadeIn(0.05f);
            _reelDirection = 0;
			_reelSpeed = ReelInitialSpeed;
			_grappleConnected = true;

            Locator.GetShipLogManager().RevealFact(TranslationHandler.GetTranslation("EH_ENTRANCE_X2", TranslationHandler.TextType.UI));

            var newCharged = BeaconController.Instance.TouchBeacon(hitInfo.collider, _charged);
			if (newCharged != _charged)
			{
				_charged = newCharged;
				if (_charged)
				{
                    //Locator.GetShipLogManager().RevealFact("");
                    _oneShotAudioSource.PlayOneShot(ChargeAudio, 1f);
					ChargeEffect.SetActive(true);
                }
				else
				{
                    Locator.GetShipLogManager().RevealFact(TranslationHandler.GetTranslation("EH_CAVERN_X3", TranslationHandler.TextType.UI));
                    _oneShotAudioSource.PlayOneShot(DischargeAudio, 1f);
                    ChargeEffect.SetActive(false);
                }
			}
        }
    }

	public void ReleaseGrapple()
	{
        Endpoint.transform.parent = transform;
        Endpoint.transform.localPosition = Vector3.zero;
        Endpoint.gameObject.SetActive(false);

		DestroyImmediate(_joint);
		DestroyImmediate(_joint2);

		_oneShotAudioSource.PlayOneShot(ReleaseAudio, 0.7f);
		_ambienceAudioSource.FadeOut(0.1f);

		_grappleConnected = false;
    }

}
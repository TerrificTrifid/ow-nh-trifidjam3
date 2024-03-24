using Mono.Cecil;
using NewHorizons;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3;

[UsedInUnityProject]
public class GrappleController : OWItem
{
	public static GrappleController Instance { get; private set; }

	public static float MaxLength = 50f;
	public static float MinLength = 1f;
	public static float ReelSpeed = 16f;
	public static float SpringStrength = 0.2f;
	public static float SpringDamper = 0.01f;
	public static float TorqueStrength = 50f;
	public static float TorqueDeadzone = 0f;

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

	public AudioClip AmbienceAudio;
	private OWAudioSource _ambienceAudioSource;
	public AudioClip ReelAudio;
    private OWAudioSource _reelAudioSource;
    public AudioClip ActivateAudio;
	public AudioClip ReleaseAudio;
	private OWAudioSource _oneShotAudioSource;

	private bool _grappleActive;
	private bool _grappleConnected;
	private int _reelDirection;
	private float _targetLength;
	private SpringJoint _joint;

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
        _ambienceAudioSource.SetMaxVolume(0.1f);
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
				//_joint.axis = Vector3.Cross(player.GetRelativeAcceleration(Endpoint.GetAttachedOWRigidbody()), player.transform.InverseTransformPoint(Endpoint.transform.position)).normalized;

                _targetLength += _reelDirection * ReelSpeed * Time.fixedDeltaTime;
				_targetLength = Mathf.Clamp(_targetLength, MinLength - 1f, MaxLength - 1f);
				_joint.maxDistance = _targetLength;
				_joint.minDistance = _targetLength;

				var endpointDir = player.transform.InverseTransformPoint(Endpoint.transform.position).normalized;
				var cameraDir = Locator.GetActiveCamera().transform.forward;
				//var torque = -(Vector3.Project(relativeEndpoint, cameraDir) * 2f - relativeEndpoint);
				var torqueDir = Vector3.Cross(endpointDir, Vector3.Cross(endpointDir, cameraDir));
				var torqueForce = 1f - Vector3.Dot(endpointDir, cameraDir);
				if (torqueForce > TorqueDeadzone)
				{
                    player.AddAcceleration(torqueDir * (torqueForce - TorqueDeadzone) * TorqueStrength);
                }
					
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
        if (Physics.Raycast(Locator.GetActiveCamera().transform.position, Locator.GetActiveCamera().transform.forward, out var hitInfo, MaxLength, OWLayerMask.groundMask))
        {
			if (hitInfo.distance < MinLength || hitInfo.rigidbody.GetAttachedOWRigidbody() == null) return;
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
			player.MoveToPosition(playerPosition + (Locator.GetActiveCamera().transform.forward * (hitInfo.distance - 1f)));
			//NHLogger.Log(hitInfo.point - Locator.GetActiveCamera().transform.forward);
			//NHLogger.Log(playerPosition + (Locator.GetActiveCamera().transform.forward * (hitInfo.distance - 1f)));

			_joint = hitInfo.rigidbody.gameObject.AddComponent<SpringJoint>();
			_joint.connectedBody = player.GetRigidbody();

			player.MoveToPosition(playerPosition);

			_joint.anchor = hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point);
			_joint.autoConfigureConnectedAnchor = false;
			_joint.connectedAnchor = Vector3.zero;
			//_joint.axis = Vector3.Cross(player.GetRelativeAcceleration(hitInfo.rigidbody.GetAttachedOWRigidbody()), player.transform.InverseTransformPoint(hitInfo.point)).normalized;
			_joint.enableCollision = true;

			_targetLength = hitInfo.distance - 1f;
            _joint.maxDistance = _targetLength;
            _joint.minDistance = _targetLength;
			_joint.spring = SpringStrength;
			_joint.damper = SpringDamper;

            _reelDirection = 0;
			_grappleConnected = true;
        }
    }

	public void ReleaseGrapple()
	{
        Endpoint.transform.parent = transform;
        Endpoint.transform.localPosition = Vector3.zero;
        Endpoint.gameObject.SetActive(false);

		DestroyImmediate(_joint);

		_grappleConnected = false;
    }

}
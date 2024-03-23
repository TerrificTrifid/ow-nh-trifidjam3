using NewHorizons;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3;

[UsedInUnityProject]
public class GrappleController : OWItem
{
	public static GrappleController Instance { get; private set; }

	private ScreenPrompt _activatePrompt;
	private ScreenPrompt _reelInPrompt;
	private ScreenPrompt _reelOutPrompt;

	public Light Light;

	public AudioClip AmbienceAudio;
	private OWAudioSource _ambienceAudioSource;
	public AudioClip ReelAudio;
    private OWAudioSource _reelAudioSource;
    public AudioClip ActivateAudio;
	public AudioClip ReleaseAudio;
	private OWAudioSource _oneShotAudioSource;

	private bool _grappleActive;
	private int _reelDirection;

	public override void Awake()
	{
		Instance = this;
		_type = ItemType.VisionTorch;
		base.Awake();
	}

	private void Start()
	{
		_activatePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, TranslationHandler.GetTranslation("Activate", TranslationHandler.TextType.UI) + "   <CMD>");
		_reelInPrompt = new ScreenPrompt(InputLibrary.thrustUp, TranslationHandler.GetTranslation("Reel In", TranslationHandler.TextType.UI) + "   <CMD>");
		_reelOutPrompt = new ScreenPrompt(InputLibrary.thrustDown, TranslationHandler.GetTranslation("Reel Out", TranslationHandler.TextType.UI) + "   <CMD>");

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
		return TranslationHandler.GetTranslation("Threader", TranslationHandler.TextType.UI);
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
		if (OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character))
		{
			if (TrifidJam3.Instance.Planet.transform.position.magnitude > 1000f)
            {
				NotificationManager.SharedInstance.PostNotification(new NotificationData(NotificationTarget.Player, TranslationHandler.GetTranslation("OUT OF RANGE", TranslationHandler.TextType.UI), 2f), false);
			}
			else if (!_grappleActive)
			{
				NHLogger.Log("activate");
				ActivateGrapple();
			}
		}
		else if (_grappleActive)
		{
			NHLogger.Log("release");
			ReleaseGrapple();
		}

		if (OWInput.IsPressed(InputLibrary.thrustUp, InputMode.Character))
		{
			NHLogger.Log("reel in");
			_reelDirection = -1;
		}
		else if (OWInput.IsPressed(InputLibrary.thrustDown, InputMode.Character))
		{
			NHLogger.Log("reel out");
			_reelDirection = 1;
		}
		else _reelDirection = 0;
	}

	private void FixedUpdate()
	{
		if (_grappleActive)
		{
			
		}
	}

	public void ActivateGrapple()
	{
		_grappleActive = true;
		_reelDirection = 0;
		Light.intensity = 0.8f;
		Light.range = 16f;
	}

	public void ReleaseGrapple()
	{
		_grappleActive = false;
		_reelDirection = 0;
        Light.intensity = 0.4f;
        Light.range = 8f;
    }

}
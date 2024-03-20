using NewHorizons;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3;

[UsedInUnityProject]
public class GrappleController : MonoBehaviour
{
	public static GrappleController Instance { get; private set; }
	
	public AudioClip AmbienceAudio;
	public AudioClip ReelInAudio;
	public AudioClip ReelOutAudio;
	private OWAudioSource _loopAudioSource;
	public AudioClip ActivateAudio;
	public AudioClip ReleaseAudio;
	private OWAudioSource _oneShotAudioSource;

	private bool _grappleActive;
	private int _reelDirection;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		var playerAudioController = Locator.GetPlayerAudioController();

        _loopAudioSource = Instantiate(
			playerAudioController._oneShotSource,
			playerAudioController._oneShotSource.transform.parent
		);
		_oneShotAudioSource = Instantiate(
			playerAudioController._oneShotSource,
			playerAudioController._oneShotSource.transform.parent
		);

		Delay.FireOnNextUpdate(() =>
		{
			gameObject.SetActive(false);
		});
	}

	private void OnDestroy()
	{
		Destroy(_loopAudioSource.gameObject);
		Destroy(_oneShotAudioSource.gameObject);
	}

	public bool IsGrappleActive() => _grappleActive;

	private void FixedUpdate()
	{
		
	}

	public void ActivateGrapple()
	{
		_grappleActive = true;
		_reelDirection = 0;
    }

	public void ReleaseGrapple()
	{
		_grappleActive = false;
		_reelDirection = 0;
    }

	public void Reel(int direction)
	{
		_reelDirection = direction;
	}
}

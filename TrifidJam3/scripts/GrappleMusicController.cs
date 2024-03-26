using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class GrappleMusicController : MonoBehaviour
    {
        public static GrappleMusicController Instance { get; private set; }

        public OWTriggerVolume Trigger;
        public AudioClip[] Music;
        private OWAudioSource[] _audioSources;
        private int _currentMusic;
        public static float FadeTime = 5f;
        private bool _isPlaying;

        private void Awake()
        {
            Instance = this;
            Trigger.OnEntry += OnEntry;
            Trigger.OnExit += OnExit;
        }

        private void Start()
        {
            var playerAudioController = Locator.GetPlayerAudioController();
            _audioSources = new OWAudioSource[Music.Length];
            for(int i = 0; i < Music.Length; i++)
            {
                _audioSources[i] = Instantiate(
                    playerAudioController._oneShotSource,
                    playerAudioController._oneShotSource.transform.parent
                );
                _audioSources[i].clip = Music[i];
                _audioSources[i].loop = true;
                _audioSources[i].SetMaxVolume(0.7f);
            }

            
        }

        private void OnEntry(GameObject hitobj)
        {
            if (_isPlaying) return;
            var body = hitobj.GetAttachedOWRigidbody();
            if (!body.CompareTag("Player")) return;

            Play(BeaconController.Instance.GetActiveCount() - 1);
            _isPlaying = true;
        }

        private void OnExit(GameObject hitobj)
        {
            if (!_isPlaying) return;
            var body = hitobj.GetAttachedOWRigidbody();
            if (!body.CompareTag("Player")) return;

            Stop();
            _isPlaying = false;
        }

        public void Play(int i)
        {
            _currentMusic = i;
            _audioSources[_currentMusic].FadeIn(FadeTime, true);
        }

        public void Stop()
        {
            _audioSources[_currentMusic].FadeOut(FadeTime);
        }

        public void SwitchTo(int i)
        {
            _audioSources[_currentMusic].FadeOut(FadeTime);
            _audioSources[i].FadeIn(FadeTime);
            _audioSources[i].time = _audioSources[_currentMusic].time;

            _currentMusic = i;
        }

        public bool IsPlaying() => _isPlaying;
    }
}

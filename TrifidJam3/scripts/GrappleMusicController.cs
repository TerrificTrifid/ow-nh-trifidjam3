using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class GrappleMusicController : MonoBehaviour
    {
        public static GrappleMusicController Instance { get; private set; }

        public AudioClip[] Music;
        private OWAudioSource[] _audioSources;
        private int _currentMusic;
        public static float FadeTime = 3f;
        private bool _isPlaying;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var playerAudioController = Locator.GetPlayerAudioController();

            for(int i = 0; i < Music.Length; i++)
            {
                _audioSources[i] = Instantiate(
                    playerAudioController._oneShotSource,
                    playerAudioController._oneShotSource.transform.parent
                );
                _audioSources[i].clip = Music[i];
                _audioSources[i].loop = true;
            }

            
        }

        private void OnEntry(GameObject hitobj)
        {
            if (_isPlaying) return;
            var body = hitobj.GetAttachedOWRigidbody();
            if (!body.CompareTag("Player")) return;

            Play(BeaconController.Instance.GetActiveCount());
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
            _audioSources[_currentMusic].FadeIn(FadeTime);
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

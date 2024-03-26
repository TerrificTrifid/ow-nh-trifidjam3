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
    }
}

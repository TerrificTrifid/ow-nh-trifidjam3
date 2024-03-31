using NewHorizons;
using NewHorizons.Handlers;
using UnityEngine;

namespace TrifidJam3
{
    public class AbstractEnding : MonoBehaviour
    {
        public static AbstractEnding Instance { get; private set; }

        public ParticleSystem Particles;
        public ParticleSystem Sillies;
        public Light AmbientLight;
        private float _ambientIntensity;
        private float _maxParticles;
        private bool _silly;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _ambientIntensity = AmbientLight.intensity;
            _maxParticles = Particles.main.startLifetimeMultiplier * Particles.emission.rateOverTimeMultiplier;
            _silly = TrifidJam3.Instance.SillyMode;
            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            AmbientLight.intensity = _ambientIntensity * (Particles.particleCount / _maxParticles);
        }

        public void TriggerEnding()
        {
            Locator.GetShipLogManager().RevealFact(TranslationHandler.GetTranslation("EH_PHOSPHORS_X1", TranslationHandler.TextType.UI));
            Locator.GetShipLogManager().RevealFact(TranslationHandler.GetTranslation("EH_PHOSPHORS_X2", TranslationHandler.TextType.UI));
            Locator.GetShipLogManager().RevealFact(TranslationHandler.GetTranslation("EH_PHOSPHORS_X3", TranslationHandler.TextType.UI));
            gameObject.SetActive(true);

            if (_silly)
            {
                Sillies.Play();
            }
            else
            {
                Particles.Play();
            }
        }

        public void SetEnding(bool silly)
        {
            if (_silly != silly && BeaconController.Instance.IsEndingTriggered())
            {
                if (silly)
                {
                    Particles.Stop();
                    Sillies.Play();
                }
                else
                {
                    Sillies.Stop();
                    Particles.Play();
                }
            }
            _silly = silly;
        }
    }
}

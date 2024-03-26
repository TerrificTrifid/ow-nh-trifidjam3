using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class AbstractEnding : MonoBehaviour
    {
        public static AbstractEnding Instance { get; private set; }

        public ParticleSystem Particles;
        public Light AmbientLight;
        private float _ambientIntensity;
        private float _maxParticles;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _ambientIntensity = AmbientLight.intensity;
            _maxParticles = Particles.main.startLifetimeMultiplier * Particles.emission.rateOverTimeMultiplier;
        }

        private void FixedUpdate()
        {
            AmbientLight.intensity = _ambientIntensity * (Particles.particleCount / _maxParticles);
        }
    }
}

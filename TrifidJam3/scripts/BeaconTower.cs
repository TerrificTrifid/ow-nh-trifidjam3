using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class BeaconTower : MonoBehaviour
    {
        public int Id;
        public Light[] Lights;
        public MeshRenderer[] Emissions;

        private float[] _lightIntensities;
        private Color[] _emissionColors;
        private bool _isLit;
        private float _fade;

        private void Awake()
        {
            
        }

        private void Start()
        {
            _lightIntensities = new float[Lights.Length];
            _emissionColors = new Color[Emissions.Length];

            for (int i = 0; i < Lights.Length; i++)
            {
                _lightIntensities[i] = Lights[i].intensity;
            }
            for (int i = 0; i < Emissions.Length; i++)
            {
                var material = Emissions[i].material;
                _emissionColors[i] = material.GetColor("_EmissionColor");
            }
        }

        private void FixedUpdate()
        {
            if (_isLit && _fade < 1f)
            {
                _fade = Mathf.Min(1f, _fade + Time.fixedDeltaTime);

            }
            else if (!_isLit && _fade > 0f)
            {
                _fade = Mathf.Max(0f, _fade - Time.fixedDeltaTime);

            }
        }

        public void Activate()
        {
            _isLit = true;
        }

        public void Deactivate()
        {
            _isLit = false;
        }
    }
}

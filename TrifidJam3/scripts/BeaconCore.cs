using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class BeaconCore : MonoBehaviour
    {
        public int Id;
        public Light[] Lights;
        public MeshRenderer[] Emissions;
        public Collider Core;

        private float[] _lightIntensities;
        private Color[] _emissionColors;
        private bool _isLit;
        private float _fade;

        private void Awake()
        {
            _isLit = true;
            _fade = 1.0f;
        }

        private void Start()
        {
            
        }

        private void FixedUpdate()
        {

        }

        public void Activate()
        {

        }
    }
}

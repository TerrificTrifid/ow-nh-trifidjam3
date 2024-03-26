using NewHorizons;
using UnityEngine;
using static RumbleManager.Rumble;

namespace TrifidJam3
{
    public class BeaconTower : MonoBehaviour
    {
        public int Id;
        public GameObject Light;
        public MeshRenderer Emission;
        public Color EmissionColor;
        public GameObject Beam;

        private void Awake()
        {
            
        }

        private void Start()
        {
            
        }

        private void FixedUpdate()
        {
            
        }

        public void Activate()
        {
            Light.SetActive(true);
            Beam.SetActive(true);
            Emission.material.SetColor("_EmissionColor", EmissionColor);
        }

        public void Deactivate()
        {
            Light.SetActive(false);
            Beam.SetActive(false);
            Emission.material.SetColor("_EmissionColor", Color.black);
        }
    }
}

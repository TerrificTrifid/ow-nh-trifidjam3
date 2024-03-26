using NewHorizons;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3
{
    public class BeaconController : MonoBehaviour
    {
        public static BeaconController Instance { get; private set; }
        public static int Amount = 4;
        public static float FadeTime = 1.0f;
        private BeaconTower[] _towers;
        private BeaconCore[] _cores;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _cores = TrifidJam3.Instance.Planet.GetComponentsInChildren<BeaconCore>();
            _towers = TrifidJam3.Instance.Planet.GetComponentsInChildren<BeaconTower>();
            if (_cores.Length < Amount) NHLogger.Log("Not enough cores found!");
            if (_towers.Length < Amount) NHLogger.Log("Not enough towers found!");
        }
    }
}

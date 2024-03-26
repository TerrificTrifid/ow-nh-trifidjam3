using NewHorizons;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3
{
    public class BeaconController : MonoBehaviour
    {
        public static BeaconController Instance { get; private set; }
        public static int BeaconAmount = 4;

        public GameObject Ending;

        private BeaconTower[] _towers;
        private BeaconCore[] _cores;
        private bool _endingTriggered;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            //_cores = TrifidJam3.Instance.Planet.GetComponentsInChildren<BeaconCore>();
            //_towers = TrifidJam3.Instance.Planet.GetComponentsInChildren<BeaconTower>();

            _towers = new BeaconTower[BeaconAmount];
            _cores = new BeaconCore[BeaconAmount];

            Delay.FireOnNextUpdate(() =>
            {
                var planet = TrifidJam3.Instance.Planet;
                _towers[0] = planet.FindChild("Sector/PlanetExterior/Beacon").GetComponent<BeaconTower>();
                _towers[1] = planet.FindChild("Sector/PlanetExterior/BeaconPivot/Beacon (1)").GetComponent<BeaconTower>();
                _towers[2] = planet.FindChild("Sector/PlanetExterior/BeaconPivot (1)/Beacon (1)").GetComponent<BeaconTower>();
                _towers[3] = planet.FindChild("Sector/PlanetExterior/BeaconPivot (2)/Beacon (1)").GetComponent<BeaconTower>();
                _cores[0] = planet.FindChild("Sector/PlanetInterior/BeaconSpirePivot/BeaconSpire/Structure/BeaconCore").GetComponent<BeaconCore>();
                _cores[1] = planet.FindChild("Sector/PlanetInterior/BeaconSpirePivot (2)/BeaconSpire/Structure/BeaconCore").GetComponent<BeaconCore>();
                _cores[2] = planet.FindChild("Sector/PlanetInterior/BeaconSpirePivot (3)/BeaconSpire/Structure/BeaconCore").GetComponent<BeaconCore>();
                _cores[3] = planet.FindChild("Sector/PlanetInterior/BeaconSpirePivot (4)/BeaconSpire/Structure/BeaconCore").GetComponent<BeaconCore>();

                for(int i = 0; i < BeaconAmount; i++)
                {
                    _towers[i].Id = i;
                    _cores[i].Id = i;
                    if (i != 0)
                    {
                        _towers[i].Deactivate();
                        _cores[i].Deactivate();
                    }
                }
            });
        }

        public int GetActiveCount()
        {
            int count = 0;
            foreach (var core in _cores)
            {
                if (core.IsLit()) count++;
            }
            return count;
        }

        public bool TouchBeacon(Collider collider, bool charged)
        {
            for (int i = 0; i < BeaconAmount; i++)
            {
                if (_cores[i].Collider.Equals(collider))
                {
                    if (_cores[i].IsLit())
                    {
                        charged = true;
                    }
                    else if (charged)
                    {
                        _cores[i].Activate();
                        _towers[i].Activate();
                        charged = false;

                        if (!_endingTriggered && GetActiveCount() == BeaconAmount)
                        {
                            TriggerEnding();
                        }
                    }
                    break;
                }
            }

            return charged;
        }

        public void TriggerEnding()
        {
            _endingTriggered = true;
        }
    }
}

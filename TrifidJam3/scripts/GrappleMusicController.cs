using NewHorizons;
using NewHorizons.Components.Props;
using UnityEngine;

namespace TrifidJam3
{
    public class GrappleMusicController : MonoBehaviour
    {
        private static NHItem nhItem;

        private void Awake()
        {
            
        }

        private void Start()
        {
            nhItem = this.GetComponent<NHItem>();
        }
    }
}

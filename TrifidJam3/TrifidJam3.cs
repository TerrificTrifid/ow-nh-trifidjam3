using OWML.Common;
using OWML.ModHelper;
using NewHorizons;
using UnityEngine;
using NewHorizons.Components.Props;

namespace TrifidJam3
{
    public class TrifidJam3 : ModBehaviour
    {
        public INewHorizons NewHorizons;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            NewHorizons.GetStarSystemLoadedEvent().AddListener(system =>
            {
                if (system != "Jam3") return;

                

                ModHelper.Console.WriteLine($"{nameof(TrifidJam3)} is ready", MessageType.Success);

            });
        }
    }
}
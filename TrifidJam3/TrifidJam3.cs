using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

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
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(TrifidJam3)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            NewHorizons.GetStarSystemLoadedEvent().AddListener(system =>
            {
                if (system != "xen.ModJam3") return;


            });
        }
    }
}
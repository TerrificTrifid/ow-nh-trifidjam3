using OWML.Common;
using OWML.ModHelper;
using NewHorizons;
using UnityEngine;
using NewHorizons.Components.Props;
using NewHorizons.Handlers;
using HarmonyLib;
using System.Reflection;
using NewHorizons.Utility;
using NewHorizons.Components;

namespace TrifidJam3
{
    public class TrifidJam3 : ModBehaviour
    {
        public static TrifidJam3 Instance;

        public INewHorizons NewHorizons;

        public GameObject Planet;

        public bool SillyMode;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.

            Instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            SillyMode = ModHelper.Config.GetSettingsValue<bool>("SillyMode");

            NewHorizons.GetStarSystemLoadedEvent().AddListener(system =>
            {
                if (system != "Jam3") return;

                Planet = NewHorizons.GetPlanet(TranslationHandler.GetTranslation("Echo Hike", TranslationHandler.TextType.UI));

                var crates = Planet.FindChild("Sector/PlanetInterior/EntranceRoot2/Interior/Crates/test");
                foreach (var crate in crates.GetAllChildren())
                {
                    var physics = crate.AddComponent<AddPhysics>();
                    physics.Sector = Planet.GetComponentInChildren<Sector>();
                    physics.Mass = 0.0005f * crate.transform.localScale.x;
                    physics.Radius = 0f;
                    physics.SuspendUntilImpact = false;
                }

                //Planet.FindChild("Sector/MainAmbience").GetComponent<OWAudioSource>().pitch = 0.5f;

            });
        }

        public override void Configure(IModConfig config)
        {
            SillyMode = config.GetSettingsValue<bool>("SillyMode");
            if (AbstractEnding.Instance != null)
            {
                AbstractEnding.Instance.SetEnding(SillyMode);
                GrappleMusicController.Instance.SetEndingMusic(SillyMode);
            }
        }
    }
}
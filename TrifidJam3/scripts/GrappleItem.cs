using HarmonyLib;
using NewHorizons;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using UnityEngine;


namespace TrifidJam3
{
    [UsedInUnityProject]
    [HarmonyPatch]
    public class GrappleItem : OWItem
    {
        private ScreenPrompt _activatePrompt;
        private ScreenPrompt _reelInPrompt;
        private ScreenPrompt _reelOutPrompt;

        public override void Awake()
        {
            // prevents scout equip
            _type = ItemType.VisionTorch;
            base.Awake();
        }

        private void Start()
        {
            _activatePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, TranslationHandler.GetTranslation("Activate", TranslationHandler.TextType.UI) + "   <CMD>");
            _reelInPrompt = new ScreenPrompt(InputLibrary.thrustUp, TranslationHandler.GetTranslation("Reel In", TranslationHandler.TextType.UI) + "   <CMD>");
            _reelOutPrompt = new ScreenPrompt(InputLibrary.thrustDown, TranslationHandler.GetTranslation("Reel Out", TranslationHandler.TextType.UI) + "   <CMD>");

            enabled = false;
        }

        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation("Threader", TranslationHandler.TextType.UI);
        }

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);

            Locator.GetPromptManager().AddScreenPrompt(_activatePrompt, PromptPosition.UpperRight, true);
            Locator.GetPromptManager().AddScreenPrompt(_reelInPrompt, PromptPosition.UpperRight, true);
            Locator.GetPromptManager().AddScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight, true);

            enabled = true;
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            
            Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_reelInPrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight);

            enabled = false;
        }

        public override void SocketItem(Transform socketTransform, Sector sector)
        {
            base.SocketItem(socketTransform, sector);
            
            Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_reelInPrompt, PromptPosition.UpperRight);
            Locator.GetPromptManager().RemoveScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight);

            enabled = false;
        }

        public override bool CheckIsDroppable()
        {
            return !GrappleController.Instance.IsGrappleActive();
        }

        private void Update()
        {
            var controller = GrappleController.Instance;

            if (OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character))
            {
                if (!controller.IsGrappleActive())
                {
                    NHLogger.Log("activate");
                    controller.ActivateGrapple();
                }
            }
            else if (controller.IsGrappleActive())
            {
                NHLogger.Log("release");
                controller.ReleaseGrapple();
            }

            if (OWInput.IsPressed(InputLibrary.thrustUp, InputMode.Character))
            {
                NHLogger.Log("reel in");
                controller.Reel(-1);
            }
            else if (OWInput.IsPressed(InputLibrary.thrustDown, InputMode.Character))
            {
                NHLogger.Log("reel out");
                controller.Reel(1);
            }
            else controller.Reel(0);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(ToolModeUI), nameof(ToolModeUI.Update))]
        private static void ToolModeUI_Update(ToolModeUI __instance)
        {
            if (OWInput.IsInputMode(InputMode.Character) && __instance._toolSwapper.GetToolMode() == ToolMode.Item)
            {
                if (__instance._toolSwapper.GetItemCarryTool().GetHeldItem() is GrappleItem)
                {
                    __instance._projectPrompt.SetVisibility(false);
                }
            }
        }
    }
}

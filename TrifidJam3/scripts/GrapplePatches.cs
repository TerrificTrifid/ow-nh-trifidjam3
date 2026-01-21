using HarmonyLib;
using UnityEngine;

namespace TrifidJam3;

[HarmonyPatch]
public class GrapplePatches : MonoBehaviour
{
    // this method expects it to be a dream lantern, so only do part of it
    [HarmonyPrefix, HarmonyPatch(typeof(ToolModeUI), nameof(ToolModeUI.Update))]
    private static bool ToolModeUI_Update(ToolModeUI __instance)
    {
        if (OWInput.IsInputMode(InputMode.Character) && __instance._toolSwapper.GetToolMode() == ToolMode.Item)
        {
            if (__instance._toolSwapper.GetItemCarryTool().GetHeldItem() is GrappleController)
            {
                __instance._mapPrompt.SetVisibility(false);
                __instance._flashlightPrompt.SetVisibility(false);
                __instance._centerFlashlightPrompt.SetVisibility(false);
                __instance._probePrompt.SetVisibility(false);
                __instance._signalscopePrompt.SetVisibility(false);
                __instance._centerTranslatePrompt.SetVisibility(false);
                __instance._centerSignalscopePrompt.SetVisibility(false);
                __instance._exitShipPrompt.SetVisibility(false);
                __instance._centerProbePrompt.SetVisibility(false);
                __instance._focusPrompt.SetVisibility(false);
                __instance._concealPrompt.SetVisibility(false);
                __instance._projectPrompt.SetVisibility(false);
                __instance._centerFocusPrompt.SetVisibility(false);
                __instance._centerConcealPrompt.SetVisibility(false);
                if (__instance._inFlashlightPromptTrigger && !PlayerState.IsFlashlightOn() && OWInput.IsInputMode(InputMode.Character))
                {
                    __instance._centerFlashlightPrompt.SetVisibility(true);
                }
                if (__instance._toolSwapper.IsTranslatorEquipPromptAllowed())
                {
                    __instance._centerTranslatePrompt.SetVisibility(true);
                }

                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DeathManager), nameof(DeathManager.CheckShouldWakeInDreamWorld))]
    private static bool DeathManager_CheckShouldWakeInDreamWorld()
    {
        if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() is GrappleController)
        {
            return false;
        }

        return true;
    }
}

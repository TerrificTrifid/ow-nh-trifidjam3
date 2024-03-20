using HarmonyLib;
using UnityEngine;

namespace TrifidJam3;

[HarmonyPatch]
public class GrapplePatches : MonoBehaviour
{
    [HarmonyPostfix, HarmonyPatch(typeof(ToolModeUI), nameof(ToolModeUI.Update))]
    private static void ToolModeUI_Update(ToolModeUI __instance)
    {
        if (OWInput.IsInputMode(InputMode.Character) && __instance._toolSwapper.GetToolMode() == ToolMode.Item)
        {
            if (__instance._toolSwapper.GetItemCarryTool().GetHeldItem() is GrappleController)
            {
                __instance._projectPrompt.SetVisibility(false);
            }
        }
    }
}

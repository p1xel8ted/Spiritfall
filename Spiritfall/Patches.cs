using System.Collections.Generic;
using System.Linq;
using Avrahamy;
using Avrahamy.GG;
using HarmonyLib;
using Product.UI;
using UnityEngine;

namespace Spiritfall
{
    [HarmonyPatch]
    public partial class Plugin
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettingsModel), nameof(SettingsModel.SupportedWindowedResolutions), MethodType.Getter)]
        [HarmonyPatch(typeof(SettingsModel), nameof(SettingsModel.SupportedFullscreenResolutions), MethodType.Getter)]
        public static bool SettingsModel_SupportedResolutions(ref SettingsModel __instance, ref List<Vector2Int> __result)
        {
            var newList = new List<Vector2Int> { new(Display.main.systemWidth, Display.main.systemHeight) };
            __instance.supportedResolutions = newList;
            __result = newList;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CompanyLogo), nameof(CompanyLogo.Update))]
        private static void CompanyLogo_Update(ref CompanyLogo __instance)
        {
            __instance.logo.gameObject.SetActive(false);
            __instance.loadingAnimation.SetActive(true);
            __instance.enabled = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettingsMenuController), nameof(SettingsMenuController.OnWindowedModeChanged))]
        [HarmonyPatch(typeof(SettingsMenuController), nameof(SettingsMenuController.OnBorderlessChanged))]
        private static void SettingsMenuController_OnWindowedModeChanged(ref SettingsMenuController __instance, ref bool value) => value = true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettingsModel), nameof(SettingsModel.OnScreenResized))]
        public static bool SettingsModel_OnScreenResized(ref SettingsModel __instance)
        {
            var maxRefreshRate = Screen.resolutions
                .Where(a => a.width == Display.main.systemWidth && a.height == Display.main.systemHeight)
                .Max(r => r.refreshRate);
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow, maxRefreshRate);
            return false;
        }
    }
}
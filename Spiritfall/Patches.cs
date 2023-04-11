using System.Collections.Generic;
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
        // This method modifies supported resolutions to only include the current system resolution.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettingsModel), nameof(SettingsModel.SupportedWindowedResolutions), MethodType.Getter)]
        [HarmonyPatch(typeof(SettingsModel), nameof(SettingsModel.SupportedFullscreenResolutions), MethodType.Getter)]
        public static bool SettingsModel_SupportedResolutions(ref SettingsModel __instance, ref List<Vector2Int> __result)
        {
            // Create a list containing the current screen resolution as the only supported resolution.
            var newList = new List<Vector2Int>
            {
                new(Display.main.systemWidth, Display.main.systemHeight)
            };
        
            // Update instance properties.
            __instance.supportedResolutions = newList;
        
            // Set the result and return false to skip the original method.
            __result = newList;
            return false;
        }
        
        // This method disables the company logo and enables the loading animation.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CompanyLogo), nameof(CompanyLogo.Update))]
        private static void CompanyLogo_Update(ref CompanyLogo __instance)
        {
            __instance.logo.gameObject.SetActive(false);
            __instance.loadingAnimation.SetActive(true);
            __instance.enabled = false;
        }
        
        // Their handling of fullscreen/bordeless etc is funky. This forces proper fullscreen borderless.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettingsMenuController), nameof(SettingsMenuController.OnWindowedModeChanged))]
        [HarmonyPatch(typeof(SettingsMenuController), nameof(SettingsMenuController.OnBorderlessChanged))]
        private static void SettingsMenuController_OnWindowedModeChanged(ref SettingsMenuController __instance, ref bool value)
        {
            value = true;
        }

        // At 3440x1440 this method in its default state sets my resolution to 3413x1440??
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettingsModel), nameof(SettingsModel.OnScreenResized))]
        public static bool SettingsModel_OnScreenResized(ref SettingsModel __instance)
        {
            // Update the current resolution to match the system resolution.
            var currentResolution = Screen.currentResolution;
            currentResolution.height = Display.main.systemHeight;
            currentResolution.width = Display.main.systemWidth;
        
            // Set the new resolution and return false to skip the original method.
            __instance.SetResolution(currentResolution);
            return false;
        }
    }
}
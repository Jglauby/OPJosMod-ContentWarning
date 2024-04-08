using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEngine.Mesh;

namespace OPJosMod_ContentWarning.ExampleMod.Patches
{
    [HarmonyPatch(typeof(Player))]
    public static class PlayerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static CustomText customText = new CustomText();

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void patchStart(Player __instance)
        {
            if (__instance.IsLocal)
            {
                customText.Start();
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void patchUpdate(Player __instance)
        {
            if (__instance.IsLocal && Input.GetKeyDown(KeyCode.L))
            {
                customText.DisplayText("display text", 3f);
            }

            customText.Update();
        }
    }
}

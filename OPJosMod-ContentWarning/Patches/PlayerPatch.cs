using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void patchUpdate(Player __instance)
        {
            if (__instance.IsLocal && Input.GetKeyDown(KeyCode.K))
            {
                if (__instance.data.dead)
                {
                    mls.LogMessage($"revived {__instance.name}");
                    __instance.CallRevive();
                }
                else
                {
                    mls.LogMessage($"killed {__instance.name}");
                    __instance.Die();
                }
            }
        }
    }
}

using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [HarmonyPatch("TakeDamage")]
        [HarmonyPostfix]
        private static void patchTakeDamage(Player __instance, ref bool damage)
        {
            mls.LogMessage($"{__instance.name} was killed!");
            __instance.Die();
        }
    }
}

using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEngine.Mesh;

namespace OPJosMod_ContentWarning.SelfRevive.Patches
{
    [HarmonyPatch(typeof(Player))]
    public static class PlayerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static bool AutoRevive = true;
        private static PlayerRagdoll localPLayer;
        private static MethodInfo ragdollMethod;
        private static bool isRagdoll;
        private static float lastCalled = Time.time;
        private static float timeDied = Time.time;
        private static CustomText customText = new CustomText();

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void patchUpdate(Player __instance)
        {
            if (__instance.IsLocal && Input.GetKeyDown(KeyCode.K))
                __instance.Die();

            if (__instance.IsLocal && Input.GetKeyDown(KeyCode.L))
            {
                if (AutoRevive)
                {
                    mls.LogMessage($"auto revive off");
                    
                    AutoRevive = false;

                    if (isRagdoll)
                        __instance.Die();
                }
                else
                {
                    mls.LogMessage($"auto revive on");
                    AutoRevive = true;
                }
            }
            
            if (isRagdoll && Time.time - timeDied > 3f)
            {
                customText.DisplayText("TESTING", 7f);
                isRagdoll = false;
            }
            else if (isRagdoll && Time.time - lastCalled > 0.25f)
            {              
                lastCalled = Time.time;
                ragdollMethod.Invoke(localPLayer, new object[1] { 1f });
            }

            customText.Update();
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void patchStart(Player __instance)
        {
            if (__instance.IsLocal)
            {
                customText.Start();
            }
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void patchAwake(Player __instance)
        {
            if (__instance.IsLocal)
            {
                localPLayer = ((Component)__instance).gameObject.GetComponent<PlayerRagdoll>();
                ragdollMethod = ((object)localPLayer).GetType().GetMethod("CallFall", BindingFlags.Instance | BindingFlags.NonPublic);
            }
        }

        [HarmonyPatch("Die")]
        [HarmonyPrefix]
        private static bool patchDie(Player __instance)
        {
            if (AutoRevive && __instance.IsLocal)
            {
                isRagdoll = true;
                lastCalled = Time.time;
                timeDied = Time.time;
                __instance.data.health = 10;
                __instance.data.remainingOxygen = __instance.data.maxOxygen / 2;

                return false;
            }

            return true;
        }
    }
}

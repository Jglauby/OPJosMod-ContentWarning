using BepInEx.Logging;
using DefaultNamespace;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using static Bot;
using static UnityEngine.Mesh;
using Object = UnityEngine.Object;

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
        private static float timeDied = Time.time;
        private static float lastPressedDieButton = Time.time;
        private static CustomText customText = new CustomText();

        private static MethodInfo ragdollMethod;
        private static PlayerRagdoll localPLayerRagdoll;
        private static float lastFloppedTime = Time.time;

        private static float stayRagdollTime = 2.5f;
        private static float timeRequiredNotPressingDeadButton = 3f;

        private static Bot[] allEnemies;
        private static float lastTimeSavedLocation = Time.time;
        private static List<Vector3> lastLocations = new List<Vector3>();

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void patchUpdate(Player __instance)
        {
            if (!__instance.IsLocal)
                return;

            handleInputs(__instance);

            if (__instance.data.dead && AutoRevive)
            {
                if (Time.time - timeDied > stayRagdollTime
                    && Time.time - lastPressedDieButton > timeRequiredNotPressingDeadButton)
                {
                    __instance.data.dead = false;
                }
                else if (Time.time - lastFloppedTime > 0.1f)
                {
                    lastFloppedTime = Time.time;
                    ragdollMethod.Invoke(localPLayerRagdoll, new object[1] { 1f });
                }
            }

            //save last existed locations
            if (Time.time - lastTimeSavedLocation > 0.1f)
            {
                //mls.LogMessage($"saved location {__instance.HeadPosition()}");
                lastTimeSavedLocation = Time.time;

                if (((lastLocations.Count > 1 && Vector3.Distance(lastLocations[lastLocations.Count - 1], __instance.HeadPosition()) > 0.2) ||
                        lastLocations.Count <= 1) && true)//ad check to being on ground instead of this "true" check
                    lastLocations.Insert(0, __instance.HeadPosition());

                if (lastLocations.Count > 1000)
                    lastLocations.RemoveAt(lastLocations.Count - 1);
            }

            customText.Update();
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void patchAwake(Player __instance)
        {
            if (__instance.IsLocal)
            {
                localPLayerRagdoll = ((Component)__instance).gameObject.GetComponent<PlayerRagdoll>();
                ragdollMethod = ((object)localPLayerRagdoll).GetType().GetMethod("CallFall", BindingFlags.Instance | BindingFlags.NonPublic);
            }
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

        [HarmonyPatch("Die")]
        [HarmonyPrefix]
        private static bool patchDie(Player __instance)
        {
            return handleDeath(__instance);
        }

        [HarmonyPatch("CallDie")]
        [HarmonyPrefix]
        private static bool patchCallDie(Player __instance)
        {
            return handleDeath(__instance);
        }

        private static bool handleDeath(Player __instance)
        {
            if (AutoRevive && __instance.IsLocal)
            {
                timeDied = Time.time;
                __instance.data.dead = true;
                __instance.data.health = 30;

                if (__instance.data.remainingOxygen < 1)
                    __instance.data.remainingOxygen = __instance.data.maxOxygen / 4;

                handleEnemies(__instance);
                
                return false;
            }

            return true;
        }

        private static void handleInputs(Player __instance)
        {
            try
            {
                if (__instance.IsLocal && Input.GetKeyDown(ConfigVariables.ReviveButton))
                {
                    lastPressedDieButton = Time.time;
                    if (!__instance.data.dead)
                    {
                        __instance.Die();                      
                    }
                    else if (AutoRevive == false)
                    {
                        if (__instance.data.remainingOxygen < 1)
                            __instance.data.remainingOxygen = __instance.data.maxOxygen / 4;

                        __instance.CallRevive();
                    }
                }
            }
            catch { }

            try
            {
                if (__instance.IsLocal && Input.GetKeyDown(ConfigVariables.AutoReviveButton))
                {
                    if (AutoRevive)
                    {
                        mls.LogMessage($"auto revive off");
                        customText.DisplayText("auto revive OFF", 3f);
                        AutoRevive = false;

                        if (__instance.data.dead)
                            __instance.Die();
                    }
                    else
                    {
                        mls.LogMessage($"auto revive on");
                        customText.DisplayText("auto revive ON", 3f);
                        AutoRevive = true;
                    }
                }
            }
            catch { }
        }

        private static void handleEnemies(Player __instance)
        {
            allEnemies = Object.FindObjectsOfType<Bot>();
            foreach (Bot enemy in allEnemies)
            {
                //mls.LogMessage($"enemy at {enemy.transform.position}");
                if (Vector3.Distance(enemy.transform.position, __instance.HeadPosition()) < 5)
                {
                    //mls.LogMessage("enemy too close");
                    foreach (var position in lastLocations)
                    {
                        //mls.LogMessage($"looping through positions {position}");
                        if (Vector3.Distance(enemy.transform.position, position) >= 5)
                        {
                            mls.LogMessage($"setting positon to {position}");
                            __instance.refs.rigRoot.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);

                            break;
                        }
                    }
                }

                //hanlde any slurpers
                Bot_Slurper slurperEnemy = enemy.GetComponent<Bot_Slurper>();
                if (slurperEnemy != null)
                {
                    FieldInfo viewField = typeof(Bot_Slurper).GetField("view_g", BindingFlags.NonPublic | BindingFlags.Instance);
                    PhotonView view_g = (PhotonView)viewField.GetValue(slurperEnemy);

                    if (slurperEnemy.playerAttached != null && slurperEnemy.playerAttached.name == __instance.name)
                    {
                        mls.LogMessage("drop playe from sluper");
                        view_g.RPC("RPCA_ReleasePlayer", RpcTarget.All, Array.Empty<object>());
                    }
                }
            }
        }
    }
}

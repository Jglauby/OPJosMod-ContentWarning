﻿using BepInEx.Logging;
using DefaultNamespace;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
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

        public static Player LocalPlayer;
        public static bool AutoRevive = true;

        private static PlayerRagdoll localPLayerRagdoll;
        private static MethodInfo ragdollMethod;
        private static bool isRagdoll;
        private static float lastCalled = Time.time;
        private static float timeDied = Time.time;
        private static CustomText customText = new CustomText();

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void patchUpdate(Player __instance)
        {
            handleInputs(__instance);
            
            if (isRagdoll && Time.time - timeDied > 2.5f)
            {              
                isRagdoll = false;

                __instance.data.dead = false;
            }
            else if (isRagdoll && Time.time - lastCalled > 0.1f)
            {              
                lastCalled = Time.time;
                ragdollMethod.Invoke(localPLayerRagdoll, new object[1] { 1f });
            }

            customText.Update();
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void patchStart(Player __instance)
        {
            if (__instance.IsLocal)
            {
                LocalPlayer = __instance;
                customText.Start();
            }
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
                isRagdoll = true;
                lastCalled = Time.time;
                timeDied = Time.time;
                __instance.data.dead = true;
                __instance.data.health = 25;

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
                    if (!__instance.data.dead)
                        __instance.Die();
                    else
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

                        if (isRagdoll)
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
            Bot[] enemies = Object.FindObjectsOfType<Bot>();
            foreach (Bot enemy in enemies)
            {
                //make enemy ignore
                if (enemy.aggro && enemy.targetPlayer != null && enemy.targetPlayer.name == __instance.name)
                {
                    enemy.IgnoreTargetFor(__instance, 5f);
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

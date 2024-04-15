using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod_ContentWarning.SelfRevive.Patches;
using UnityEngine;

namespace OPJosMod_ContentWarning.SelfRevive
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.SelfRevive";
        private const string modName = "SelfRevive";
        private const string modVersion = "1.4.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static OpJosMod Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("mod has started!");
            setupConfig();

            PlayerPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configReviveButton = Config.Bind("Revive / KYS Button",
                                        "ReviveButton",
                                        KeyCode.K,
                                        "Toggles between KYS-ing and reviving");

            var configAutoReviveButton = Config.Bind("Auto Revive Toggle",
                                        "AutoReviveButton",
                                        KeyCode.L,
                                        "Toggles on and off auto revive");

            var configTeleportToSaferLocation = Config.Bind("Teleport to Safer Location",
                                        "TeleportToSaferLocation",
                                        true,
                                        "When this is on, and you have autorevive on, then when you are standign back up as you are reviving it will teleport you to a safer location from eneimes. The goal is to help prevent you from getting stuck.");

            var configSafteyRange = Config.Bind("Saftey Range",
                                        "SafteyRange",
                                        5f,
                                        "When you have the teleport to safer location on, what is the distance that is deemed safe? How far away you will appear from an enemy.");

            ConfigVariables.ReviveButton = configReviveButton.Value;
            ConfigVariables.AutoReviveButton = configAutoReviveButton.Value;
            ConfigVariables.TeleportToSaferLocation = configTeleportToSaferLocation.Value;
            ConfigVariables.SafteyRange = configSafteyRange.Value;

            Config.Save();
        }
    }
}

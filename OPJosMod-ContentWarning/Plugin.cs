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
        private const string modVersion = "1.3.1";

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


            ConfigVariables.ReviveButton = configReviveButton.Value;
            ConfigVariables.AutoReviveButton = configAutoReviveButton.Value;

            Config.Save();
        }
    }
}

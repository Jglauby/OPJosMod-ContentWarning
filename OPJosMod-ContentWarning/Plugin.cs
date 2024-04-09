using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod_ContentWarning.SelfRevive.Patches;

namespace OPJosMod_ContentWarning.SelfRevive
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.SelfRevive";
        private const string modName = "SelfRevive";
        private const string modVersion = "1.2.0";

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

            PlayerPatch.SetLogSource(mls);

            harmony.PatchAll();
        }
    }
}

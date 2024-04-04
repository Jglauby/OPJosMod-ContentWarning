﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod_ContentWarning.ExampleMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod_ContentWarning
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.ExampleMod";
        private const string modName = "ExampleMod";
        private const string modVersion = "1.0.0";

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

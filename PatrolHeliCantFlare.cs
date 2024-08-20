/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using HarmonyLib;
using Newtonsoft.Json;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("Patrol Heli Cant Flare", "VisEntities", "1.1.0")]
    [Description("Disables the flare functionality for patrol helicopters when targeted by homing missiles.")]
    public class PatrolHeliCantFlare : RustPlugin
    {
        #region Fields

        private static PatrolHeliCantFlare _plugin;
        private static Configuration _config;
        private Harmony _harmony;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Percentage Chance That Heli Cannot Use Flares")]
            public int PercentageChanceThatHeliCannotUseFlares { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            if (string.Compare(_config.Version, "1.1.0") < 0)
            {
                _config.PercentageChanceThatHeliCannotUseFlares = defaultConfig.PercentageChanceThatHeliCannotUseFlares;
            }

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                PercentageChanceThatHeliCannotUseFlares = 100
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            _harmony = new Harmony(Name + "PATCH");
            _harmony.PatchAll();
        }

        private void Unload()
        {
            _harmony.UnpatchAll(Name + "PATCH");
            _config = null;
            _plugin = null;
        }

        private object CanPatrolHelicopterFlare(PatrolHelicopter patrolHelicopter)
        {
            if (patrolHelicopter == null)
                return null;

            if (ChanceSucceeded(_config.PercentageChanceThatHeliCannotUseFlares))
                return false;

            return null;
        }

        #endregion Oxide Hooks

        #region Helper Functions

        private bool ChanceSucceeded(int chance)
        {
            return UnityEngine.Random.Range(0, 100) < chance;
        }

        #endregion Helper Functions

        #region Harmony Patches

        [HarmonyPatch(typeof(PatrolHelicopter), "OnEntityMessage")]
        public static class PatrolHelicopter_OnEntityMessage_Patch
        {
            public static bool Prefix(PatrolHelicopter __instance, BaseEntity from, string msg)
            {
                if (msg == "RadarLock")
                {
                    object result = Interface.CallHook("CanPatrolHelicopterFlare", __instance);
                    if (result is bool && !(bool)result)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion Harmony Patches
    }
}
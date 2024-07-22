/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using HarmonyLib;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("Patrol Heli Cant Flare", "VisEntities", "1.0.0")]
    [Description("Disables the flare functionality for patrol helicopters when targeted by homing missiles.")]
    public class PatrolHeliCantFlare : RustPlugin
    {
        #region Fields

        private static PatrolHeliCantFlare _plugin;
        private Harmony _harmony;

        #endregion Fields

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
            _plugin = null;
        }

        private object CanPatrolHelicopterFlare(PatrolHelicopter patrolHelicopter)
        {
            if (patrolHelicopter == null)
                return null;

            return false;
        }

        #endregion Oxide Hooks

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
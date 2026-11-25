using System;
using System.Collections.Generic;
using _200_Dev.Items.Tools;
using HarmonyLib;
using Items;
using Mirror;

namespace InvokyrAmmo
{
    /// <summary>
    /// The gun and the flamethrower are <see cref="BatteryTool"/>s: ammo is a float level in units (synced as
    /// <c>batteryLevel</c>) out of <c>batteryCapacity</c>, which comes from the prefab and isn't synced.
    /// <list type="bullet">
    /// <item>The gun is Discrete: <c>SrvTryBeginBatteryUse</c> spends 1 unit per shot inline.</item>
    /// <item>The flamethrower is Continuous: <c>SrvTryBeginBatteryUse</c> starts draining and <c>BatteryTool.Update</c>
    /// drains <c>batteryDrainPerSecond</c> on the server.</item>
    /// <item><c>OnStartServer</c> sets the level to <c>batteryStartLevel01 * batteryCapacity</c>, so scaling the capacity
    /// before it runs scales the ammo a new tool spawns with, and refills use the new capacity too.</item>
    /// </list>
    /// Host only (<see cref="NetworkServer.active"/>); clients get the level through Mirror.
    /// </summary>
    internal static class Ammo
    {
        /// <summary>Prefab capacity per tool type, recorded the first time one is seen, so repeated spawns don't compound.</summary>
        private static readonly Dictionary<string, float> BaseCapacity = new Dictionary<string, float>();

        public static void Apply(Harmony harmony)
        {
            Patch(harmony, nameof(BatteryTool.OnStartServer), prefix: nameof(OnStartServerPrefix));
            Patch(harmony, nameof(BatteryTool.SrvTryBeginBatteryUse), prefix: nameof(TryBeginPrefix));
            Patch(harmony, nameof(BatteryTool.SrvTryConsumeBattery), prefix: nameof(TryConsumePrefix));
            Patch(harmony, nameof(BatteryTool.Update), prefix: nameof(UpdatePrefix));
            Patch(harmony, nameof(BatteryTool.DeserializeSyncVars), postfix: nameof(DeserializePostfix));
        }

        private static void Patch(Harmony harmony, string method, string prefix = null, string postfix = null)
        {
            var target = AccessTools.Method(typeof(BatteryTool), method);
            if (target == null)
            {
                Plugin.Log.LogError($"BatteryTool.{method} not found; the game may have changed");
                return;
            }
            harmony.Patch(target,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(Ammo), prefix),
                postfix: postfix == null ? null : new HarmonyMethod(typeof(Ammo), postfix));
        }

        private static Plugin.WeaponConfig ConfigFor(BatteryTool tool)
        {
            if (tool == null)
                return null;
            if (tool.TryCast<HunterGunTool>() != null)
                return Plugin.Gun;
            if (tool.TryCast<FlamethrowerTool>() != null)
                return Plugin.Flamethrower;
            return null;
        }

        private static bool HostApplies(BatteryTool tool, out Plugin.WeaponConfig config)
        {
            config = null;
            if (!Plugin.Enabled.Value || !NetworkServer.active)
                return false;
            config = ConfigFor(tool);
            return config != null;
        }

        private static bool Infinite(BatteryTool tool) => HostApplies(tool, out var config) && config.Infinite.Value;

        private static void OnStartServerPrefix(BatteryTool __instance)
        {
            try
            {
                if (!HostApplies(__instance, out var config))
                    return;
                var key = __instance.GetIl2CppType().FullName;
                if (!BaseCapacity.TryGetValue(key, out var baseCapacity))
                    BaseCapacity[key] = baseCapacity = __instance.batteryCapacity;
                var discrete = __instance.drainMode == EBatteryDrainMode.Discrete;
                var capacity = AmmoMath.ScaledCapacity(baseCapacity, config.Multiplier.Value, discrete);
                if (capacity != __instance.batteryCapacity)
                {
                    __instance.batteryCapacity = capacity;
                    Plugin.Log.LogInfo($"{key}: capacity {baseCapacity} -> {capacity}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Scaling ammo failed: {e}");
            }
        }

        /// <summary>Infinite: top up before use; a discrete shot then succeeds without spending anything.</summary>
        private static bool TryBeginPrefix(BatteryTool __instance, ref bool __result)
        {
            try
            {
                if (!Infinite(__instance))
                    return true;
                if (__instance.SrvLevel < __instance.batteryCapacity)
                    __instance.SrvSetBatteryLevel(__instance.batteryCapacity);
                if (__instance.drainMode != EBatteryDrainMode.Discrete)
                    return true; // continuous: let it start draining; UpdatePrefix stops the drain
                __result = true;
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Infinite ammo failed: {e}");
                return true;
            }
        }

        private static bool TryConsumePrefix(BatteryTool __instance, ref bool __result)
        {
            if (!Infinite(__instance))
                return true;
            __result = true;
            return false;
        }

        /// <summary>BatteryTool.Update only drains a continuous tool on the server, so skipping it keeps the fuel full.</summary>
        private static bool UpdatePrefix(BatteryTool __instance) => !Infinite(__instance);

        /// <summary>Client side: show the host's larger capacity so the bar and the max count are right.</summary>
        private static void DeserializePostfix(BatteryTool __instance)
        {
            try
            {
                if (NetworkServer.active || __instance == null || ConfigFor(__instance) == null)
                    return;
                var capacity = AmmoMath.ClientCapacity(__instance.batteryCapacity, __instance.batteryLevel);
                if (capacity != __instance.batteryCapacity)
                    __instance.batteryCapacity = capacity;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Updating the client's ammo capacity failed: {e}");
            }
        }
    }
}

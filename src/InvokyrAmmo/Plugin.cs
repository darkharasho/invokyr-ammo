using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace InvokyrAmmo
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginGuid = "com.darkharasho.invokyr.ammo";
        public const string PluginName = "InvokyrAmmo";
        public const string PluginVersion = "1.0.0";

        internal static new ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static WeaponConfig Gun;
        internal static WeaponConfig Flamethrower;

        internal sealed class WeaponConfig
        {
            public ConfigEntry<bool> Infinite;
            public ConfigEntry<float> Multiplier;
        }

        public override void Load()
        {
            Log = base.Log;
            Enabled = Config.Bind("General", "Enabled", true, new ConfigDescription(
                "Apply the ammo settings below. The host's settings decide for everyone; only the host needs the mod (clients with it see the right ammo count).",
                null, new ConfigurationManagerAttributes { DispName = "Enabled", Order = 10 }));
            Gun = Bind("Gun", "shots", "Hunter gun");
            Flamethrower = Bind("Flamethrower", "fuel", "Flamethrower");

            Ammo.Apply(new Harmony(PluginGuid));
            Log.LogInfo($"{PluginName} {PluginVersion} loaded");
        }

        private WeaponConfig Bind(string section, string unit, string display) => new WeaponConfig
        {
            Infinite = Config.Bind(section, "Infinite", false, new ConfigDescription(
                $"{display} never runs out of {unit}.",
                null, new ConfigurationManagerAttributes { DispName = $"{display}: infinite {unit}", Order = 2 })),
            Multiplier = Config.Bind(section, "Multiplier", 1f, new ConfigDescription(
                $"Multiplies the {display.ToLowerInvariant()}'s {unit} (2 = twice as much). Applies to {display.ToLowerInvariant()}s spawned after the change.",
                new AcceptableValueRange<float>(AmmoMath.MinMultiplier, AmmoMath.MaxMultiplier),
                new ConfigurationManagerAttributes { DispName = $"{display}: {unit} multiplier", Order = 1 })),
        };
    }
}

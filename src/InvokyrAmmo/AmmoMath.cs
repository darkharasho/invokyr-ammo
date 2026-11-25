using System;

namespace InvokyrAmmo
{
    /// <summary>Game-independent math, so it can be unit tested.</summary>
    internal static class AmmoMath
    {
        public const float MinMultiplier = 0.1f;
        public const float MaxMultiplier = 50f;

        /// <summary>
        /// Capacity for a tool whose prefab capacity is <paramref name="baseCapacity"/>. Discrete tools (the gun) spend
        /// one unit per shot, so their capacity is rounded to whole shots and never drops below one.
        /// </summary>
        public static float ScaledCapacity(float baseCapacity, float multiplier, bool discrete)
        {
            if (baseCapacity <= 0f || float.IsNaN(multiplier))
                return baseCapacity;
            var m = Math.Clamp(multiplier, MinMultiplier, MaxMultiplier);
            var scaled = baseCapacity * m;
            return discrete ? Math.Max(1f, (float)Math.Round(scaled, MidpointRounding.AwayFromZero)) : scaled;
        }

        /// <summary>
        /// The capacity a client should show. The host scales capacity, but only the level is synced, so a client
        /// raises its own capacity to the highest level it has seen.
        /// </summary>
        public static float ClientCapacity(float localCapacity, float syncedLevel) =>
            syncedLevel > localCapacity ? syncedLevel : localCapacity;
    }
}

## Project Context

A BepInEx 6 (BepInEx.Unity.IL2CPP, be.755) mod for Invokyr (Unity 6000.3, IL2CPP, Mirror): infinite ammo and an ammo multiplier for the hunter gun and the flamethrower, host-authoritative. Game code is in `vhs.dll`. Build setup is shared with `invokyr-admin-menu` (see its CLAUDE.md for refs, packaging and the flat zip layout HML needs).

## How it works (`Ammo.cs`)
Found from GameAssembly.dll disassembly (the interop has no method bodies). Offsets are as of the Nov 19 2026 build.
- `HunterGunTool` and `FlamethrowerTool` are `Items.BatteryTool`s. Ammo is `batteryLevel` (SyncVar, units) / `_srvLevel` out of `batteryCapacity` (prefab field, not synced). Client UI: `BatteryLevel01 = level / capacity`, `BatteryChargesLeft = round(level)`, `BatteryMaxCharges = round(capacity)`.
- Gun = `Discrete`: `HunterGunTool.SrvUse` → `SrvTryBeginBatteryUse`, which subtracts 1 unit inline (doesn't call `SrvTryConsumeBattery`).
- Flamethrower = `Continuous`: `SrvUseHoldStart` → `SrvTryBeginBatteryUse` sets `_srvDraining`; `BatteryTool.Update` (server) drains `batteryDrainPerSecond * dt`, empties at 0. `SrvUseHoldEnd` → `SrvEndBatteryUse`.
- `OnStartServer` sets `_srvLevel = batteryStartLevel01 * batteryCapacity`. The multiplier scales `batteryCapacity` in a prefix there, from the first-seen prefab capacity per type.
- Infinite: top up and skip in `SrvTryBeginBatteryUse` (discrete), skip `SrvTryConsumeBattery`, skip `BatteryTool.Update` for these tools.
- Clients with the mod raise their local capacity to the highest synced level (`DeserializeSyncVars` postfix).

## Building
`scripts/package.sh` → `dist/darkharasho-InvokyrAmmo-<version>.zip`; `hml install nexus-invokyr <zip> --profile Dev`. Tests: `dotnet test tests/InvokyrAmmo.Tests`. Run builds with `nice -n 19 taskset -c 0-3`. Release: bump `Plugin.cs`, csproj, manifest, CHANGELOG; push tag `v<version>`.

# Invokyr Ammo

Adjust ammo for the hunter gun and the flamethrower in Invokyr.

- **Infinite**: the weapon never runs out (shots for the gun, fuel for the flamethrower).
- **Multiplier**: more (or less) ammo. 2 = twice as many shots or twice the fuel. Gun shots are rounded to whole shots.

Each weapon has its own settings in `BepInEx/config/com.darkharasho.invokyr.ammo.cfg`:

| Section | Setting | Default |
|---|---|---|
| General | Enabled | true |
| Gun | Infinite | false |
| Gun | Multiplier | 1 (0.1 to 50) |
| Flamethrower | Infinite | false |
| Flamethrower | Multiplier | 1 (0.1 to 50) |

A multiplier change applies to weapons that spawn after it.

## Multiplayer
The host's settings decide for everyone, and only the host needs the mod. Clients without it still get the extra ammo, but with a multiplier above 1 their ammo bar may read full until the ammo drops below the normal amount. Clients with the mod see the right count.

Requires BepInEx 6 (IL2CPP).

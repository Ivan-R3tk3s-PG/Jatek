# NEON BREACH 1.1 — PHANTOM CIRCUIT

A complete **C# / .NET 8 / Raylib-cs 8.1.0** sci-fi 2D run-and-gun platformer, upgraded from the original NEON BREACH. Entirely original procedural graphics and generated WAV sound effects: no downloaded sprites, fonts or paid assets.

## Quick start (Windows)

1. Install the **.NET 8 SDK** from https://dotnet.microsoft.com/download/dotnet/8.0 (the SDK, not just the runtime).
2. Extract the whole `NEON_BREACH` folder from the ZIP.
3. Double-click **`VERIFY-WINDOWS.bat`** first. It restores packages, builds Release, and runs 15 headless logic/asset smoke checks.
4. When verification passes, double-click **`RUN-WINDOWS.bat`** to play.

Alternatively, open **`NEON_BREACH.sln`** in Visual Studio 2022+ or use:

```powershell
dotnet restore NEON_BREACH.csproj
dotnet build NEON_BREACH.csproj -c Release
dotnet run --project NEON_BREACH.csproj -c Release --no-build -- --self-test
dotnet run --project NEON_BREACH.csproj -c Release
```

For a distributable self-contained Windows x64 build:

```powershell
dotnet publish NEON_BREACH.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

### All controls

| Control | Action |
|---|---|
| A / D or left / right arrows | Move |
| Space or W | Jump (tap for lower jump) |
| Left Shift or Q | **Phase Dash** — quick direction-based evasive move; ~1.05 s cooldown (shorter with Combat Accelerator) |
| Left Ctrl or C (while grounded and moving) | **Combat Slide** — low-profile movement and reduced hitbox |
| Mouse | Aim |
| Hold left mouse button | Fire current weapon |
| 1 / 2 / 3 or mouse wheel | Select owned weapon |
| R (during mission) | Reload current weapon |
| E | Open upgrade terminal / extract at the exit |
| 1 / 2 / 3 (in terminal) | Purchase corresponding upgrade |
| Esc | Pause / resume / close terminal |
| R or Enter (after death) | Restart mission |
| Enter on title screen | Start |

### New in 1.1 — Phantom Circuit

**Movement**

- Directional phase dash (airborne or grounded) with cooldown, cyan afterimages, speed streaks and brief damage avoidance. It is cancelled by a solid wall and by jumping. Dashing temporarily pauses shooting.
- Combat slide (ground only), using a real **28 × 27** collider rather than the standing **28 × 56** collider. Keeps feet on the floor, has a dedicated low-profile character pose and dust effects. It can dodge higher projectiles. It ends safely when there is standing room.
- Short original `dash.wav` and `slide.wav` effects.

**Combat**

- Enemy hits now cause a brief white impact flash, sparks, a stagger interruption, and physical knockback for movable enemies. The heavy has stagger throttling to avoid permanent stun-lock.
- **11 orange volatile barrels**, including paired barrels for visible chain reactions. Shots from either side can detonate them; explosions damage nearby enemies AND the player. Each barrel is removed from projectile collision after it explodes and leaves smoking debris.
- Animated expanding shockwave rings and more explosion debris; both particle and shockwave lists are capped.

**Visuals and UI**

- A new parallax maintenance layer: industrial towers, turbine housings, spinning fans, cables, coolant pipes, monitors and section-specific lighting.
- Animated world-space reactor machinery, warning accents and subtle foreground conduits.
- Additional player armor details, a new slide silhouette and dash ghosting.
- Updated mission-title identity, framed weapon slots, sector markers, damage-history health bar, active ability status and live dash cooldown meter, plus subtle screen-edge vignette.

**Original mission preserved**

- Arrival, Security Wing, Industrial Core, Lockdown — all original level geometry, jumps, gaps and checkpoint rules.
- Three original weapons: V9 Sidearm, Viper SMG, Ravager Shotgun, with original pickups, reloading, ammo and combat stats.
- Scout Drones, Security Turrets and Heavy Enforcers (final lockdown enemies still unlock extraction).
- Existing energy-credit drops, healing, ammo drops, 3 upgrades, health, death/restart, title, pause, upgrade and victory states.

### Mission route

1. **Arrival:** learn aiming, platforming, dash/slide and shoot the first barrel.
2. **Security Wing:** reach elevated platforms, collect Viper SMG and fight turrets.
3. **Industrial Core:** obtain Ravager Shotgun and use the upgrade terminal at x=4180.
4. **Lockdown:** survive stronger encounters, exploit explosive barrels, eliminate all three final enemies and press **E** at the gate near x=7450.

All three purchasable upgrades still work: reinforced armor (+60 max HP + healing), overcharged rounds (+5 damage), combat accelerator (faster movement and firing).

### Architecture (easy to explain at school)

| File | Responsibility |
|---|---|
| `Program.cs` | Entry point, window and `--self-test` mode |
| `Game.cs` | Game loop, game states, progression and object lifecycle |
| `Player.cs` | Movement physics, slide/dash state machine, collisions, aim, animated armor |
| `Barrel.cs` | Volatile barrel health, chain explosions, area damage, smoke art |
| `Enemy.cs` | Three enemy AI behaviors, damage, stagger, hit reaction, enemy artwork |
| `Projectile.cs` | Fast swept-line projectile collision against walls, barrels and characters |
| `Weapon.cs` | Gun stats, fire patterns, magazine, reload and pickups |
| `Level.cs` | Deterministic level collision geometry, four-section scenery and parallax |
| `Effects.cs` | Bounded particle list, expanding shockwaves, drops and damage numbers |
| `Ui.cs` | Menus, screens, HUD, dash meter and notifications |
| `Palette.cs` | Drawing primitives, neon palette and reusable UI elements |
| `AudioBank.cs` | Optional sound loading with safe fallback |
| `SelfTests.cs` | Logic/asset smoke checks, no window or audio needed |

### Known limitations / testing status

- **The source package is complete but not precompiled**. The authoring environment did **not** have the .NET SDK and had no network access to install it, so it could not perform a real compile, render or gameplay playtest. **`VERIFY-WINDOWS.bat` is provided to build and run the automated checks on your Windows PC.** Static source and file-integrity checks were performed, but cannot replace an actual Windows build and gameplay test.
- Window resolution remains 1280 × 720; game controls assume a fixed window size.
- The four-section mission is not a multi-level campaign. The final threat is still a stronger Heavy Enforcer rather than a custom boss.
- No save system: restarting creates a new run; checkpoints protect against falling into pits during a run.
- Generated procedural art is intentionally not a hand-painted sprite sheet; it works without asset downloads.
- Audio remains optional, so a missing audio device will not prevent gameplay.

### License / dependencies

All included project source and `Assets/*.wav` were created for this project; no external art packs are required. Raylib-cs 8.1.0 is fetched from NuGet on first restore and brings its own native raylib runtime.

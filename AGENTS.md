# Udder Destruction Agent Guide

## Project Context

- Project root: `C:\Unity\Udder Destruction`
- Unity version: `6000.3.7f1`
- Primary scene: `Assets/Scenes/SampleScene.unity`
- Test scene: `Assets/Scenes/test scene.unity`
- Main runtime namespace: `UdderDestruction`

Udder Destruction is a top-down survivor-style Unity game. The player controls Moolissa, powers fire automatically, enemies spawn in waves, and Bovinity level-ups offer random power choices.

## Working Rules

- Do not revert, reset, or overwrite user changes unless explicitly asked.
- Always inspect `git status` before staging or committing.
- Include required Unity `.meta` files when adding assets or scripts.
- Do not commit generated logs, temporary files, `Library/`, `Temp/`, or unrelated handover notes unless requested.
- Prefer existing project patterns over new architecture unless the change clearly needs a new abstraction.
- Keep changes scoped to the gameplay request and nearby systems.

## Unity-Specific Rules

- Prefer editable scene GameObjects and prefabs for visuals, UI, colliders, enemies, powers, and tuning.
- Avoid procedural-only visual objects when a prefab or scene object would let the user tune it in Unity.
- Respect manually edited scene and prefab values. Do not casually overwrite scale, colors, colliders, positions, or assigned sprites at runtime.
- If Unity is open and blocks asset, scene, prefab, or Git operations, stop and ask the user to close it before retrying.
- Broad scene builder methods may overwrite manual Unity edits. Prefer narrow builders or direct targeted edits.

## Style And UI

- Normal UI uses the Fantasy Wooden GUI Free asset pack.
- Normal UI text uses `Assets/UdderDestruction/BMYEONSUNG_TMP.asset`.
- Combat/battle text uses PixelBattleText.
- Do not display generic power-name callouts during combat. Use meaningful status or event callouts only.
- The main menu and HUD should remain editable Canvas objects.

## Key Files

- `Assets/UdderDestruction/Scripts/UdderGameController.cs`: central game loop, waves, spawning, bosses, drops, auto-aim, UI overlays, battle text, pond contamination, and power choices.
- `Assets/UdderDestruction/Scripts/UdderPlayer.cs`: player movement, HP, Cheese It, drowning, power levels, passive effects, attack timers, and pickup collection.
- `Assets/UdderDestruction/Scripts/UdderEnemy.cs`: enemy movement, targeting, water avoidance, statuses, boss behavior, contact combat, death sequence, and boss health bars.
- `Assets/UdderDestruction/Scripts/UdderProjectile.cs`: milk projectiles and homing projectile behavior.
- `Assets/UdderDestruction/Scripts/UdderHud.cs`: HUD bars, status text, power choices, and hover description panel.
- `Assets/UdderDestruction/Scripts/UdderWorldStreamer.cs`: finite grass arena, pond border, water registration, and polluted pond visuals.
- `Assets/UdderDestruction/Scripts/UdderPersistence.cs`: persistent stats and achievements.
- `Assets/UdderDestruction/Editor/UdderPrototypeSceneBuilder.cs`: editor builders for scenes, prefabs, and selected runtime assets.

## Gameplay Conventions

- Powers are enum-driven in `MilkMode.cs` and described in `UdderPlayer.cs`.
- Maximum power level is currently 10.
- The player starts with Stomp level 1.
- Boss waves occur every 10 waves.
- Normal enemy wave count, speed, and composition are calculated in `UdderGameController`.
- Status effects are generally applied in `UdderEnemy`.
- New runtime objects should be prefab-friendly and avoid hardcoding tunable values where practical.

## Verification

Preferred verification after C# or asset-reference changes:

1. Check whether Unity is already running.
2. Run Unity batch compile when possible:
   `& 'C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Unity\Udder Destruction' -logFile 'C:\Unity\Udder Destruction\precommit_compile.log'`
3. Inspect `precommit_compile.log` for `error CS`, `Compilation failed`, `Build failed`, and unexpected exceptions.
4. If changing visuals, physics, targeting, or scenes, prefer a Unity playtest or targeted scene/prefab inspection.

Notes:
- `dotnet build .\Assembly-CSharp.csproj --no-restore` may fail when Unity temp NuGet assets are missing. Treat Unity batch compile as the more reliable project check.
- Do not commit compile logs unless the user asks.

## Git And Commit Checklist

- Run `git status --short`.
- Stage only relevant files.
- Include `.meta` files for new scripts/assets.
- Verify staged files with `git diff --cached --name-only`.
- Avoid committing unrelated local notes such as `PROJECT_HANDOVER.txt` unless requested.
- After committing, push the current branch only when the user asks.

## Current Risk Areas

- `UdderGameController.cs` is large and carries many responsibilities.
- Some UI overlays and boss visuals are still procedural.
- Some runtime factory code still overwrites prefab tuning.
- Game-over restart/new-game flow is incomplete.
- Menu items such as Sound Settings, Info, and About may be placeholders.

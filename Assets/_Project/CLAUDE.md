# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HonkShot is a 2D roguelike-style Unity game where the player controls a goose that transforms between two states:
- **Idle/Shooter State**: Humanoid form for interaction and shooting
- **Ball State**: Rolls and bounces around the level

## Code Style
- Even if formula uses numbers, it is important to remember, that formulas could be tweaked by gamedesigners, so it is better to introduce variable for tweaking.
- Try to reduce comments for methods, variables, classes for programmers in ide.
- Try to avoid FindObjectOfType and comparation of monobehaviours with null, as you can use just if(component) instead of
if(component==null). These methods are performance heavy so you need to do it carefully.
- For events, if we have event of EnemyClass called Died, the subscriber method should be EnemyClass.Died+= EnemyClass_Died; 

## Architecture

### Dependency Injection (Zenject)
The project uses Zenject for DI. Key installers:
- `ConfigInstaller` - Binds config ScriptableObjects
- `GameMonoInstaller` / `ProjectMonoInstaller` - Global bindings

### Core Systems

**Configuration System** (`Scripts/Config/`)
- ScriptableObjects define stats: `PlayerConfig`, `EnemyConfig`, `EnvironmentConfig`
- `GameConfigManager` applies configs to prefabs/scene objects using reflection
- Editor buttons available for Apply/Load/Save operations

**Stat Modifier System**
- `NumericStatModifierSystem` - Handles additive/multiplicative bonuses with ID, type (Add/Mult), value, and order
- `BoolStatModifierSystem` - Boolean state modifiers (invincibility, invisibility)
- Used extensively by items and health systems

**Player State Machine**
- `PlayerStatus` tracks state (Idle/Ball/Swapping)
- Events: `OnSwapToBall`, `OnSwapToIdle`
- `PlayerBallMovement` handles physics and throw mechanics

**Health & Combat** (`Scripts/Health/`)
- `HealthController` - Damage, shields, invincibility with modifier support
- Events: `OnDied`, `OnDamaged`, `OnTakeDamageTriggered`
- Integrates with `ShieldController`

**Level System** (`Scripts/LevelSystem/`)
- `LevelManager` - Orchestrates floor/room progression
- `LevelGenerator` - Creates rooms from pools
- Events: `EnteredRoom`, `CompletedRoom`

**Score System** (`Scripts/ScoreSystem/`)
- Formula: `(DifficultyRating * SpeedMultiplier + DamageDealt/10 + ItemBuffs) * TimeCoefficient`
- `ScoreManager` listens to `LevelManager` and enemy death events

**Item System** (`Scripts/Items/`)
- `PlayerItemSO` - ScriptableObject base for items
- `ItemManager` - Loads from Resources, provides random selection
- Items apply modifiers via `InitItem()` at pickup

**Progress & Save** (`Scripts/Progress/`)
- `ProgressData` - Serializable save structure
- `ProgressSaver` - Uses ES3 plugin for persistence

### System Interactions

Level progression: `LevelManager.EnterFloor()` → generates rooms → `EnterRoom()` → player kills enemies → `ScoreManager.OnEnemyKilled()` → room completed → portal transition

Item flow: `ItemManager.GetRandomItems()` → `Item.InitItem()` → modifiers applied to relevant systems

### Directory Structure

```
Scripts/
├── Audio/          # AudioManager singleton, sound channels
├── Camera/         # CameraShakeHandler, LevelFollow
├── Config/         # ScriptableObject configs and GameConfigManager
├── Enemies/        # Enemy AI, health, attacks, bosses
├── Health/         # Health/damage/shield with modifier patterns
├── Hub/            # Hub scene, NPCs, dialogue
├── Items/          # Item system, stat modifiers
├── LevelSystem/    # Level generation, rooms, floors
├── Managers/       # TimeManager, Zenject installers
├── Player/         # Movement, states, input, fire system
├── PointSystem/    # Attack damage calculation
├── Progress/       # Save data, tutorials, score persistence
├── ScoreSystem/    # Runtime score calculation
├── Services/       # Localization
└── UI/             # Views, item selection, stats display
```

## Conventions

**Naming**
- Private fields: `_camelCase`
- Public properties: `PascalCase`
- Namespaces: `Scripts.Player`, `Scripts.Enemies`, etc.

**Events**
- Pattern: `On[EventName]` (e.g., `OnEnemyKilled`, `EnteredRoom`)
- Always unsubscribe in `OnDestroy()`

**Serialization**
- `[SerializeField]` for editor visibility
- Odin Inspector attributes for advanced UI (`[Button]`, `[Range]`)

## Third-Party Dependencies

- **Zenject** - Dependency injection
- **DoTween** - Animation tweening
- **Odin Inspector** - Editor enhancements
- **Easy Save 3 (ES3)** - Save persistence
- **NavMeshPlus** - 2D pathfinding

## Common Tasks

**Adding a New Item**
1. Create class inheriting from `Item` in `Scripts/Items/PermanentItems/`
2. Create ScriptableObject with `PlayerItemSO`
3. Override `InitItem()` to apply modifiers
4. Add to `Resources/Data/Items/` folder

**Adjusting Enemy Stats**
1. Edit `EnemyConfig` ScriptableObject
2. Add/edit `EnemyConfigData` for enemy ID
3. Use `GameConfigManager` "Apply Configs" button

**Adding Stat Modifiers**
1. Get reference to modifier system (e.g., `HealthController.MaxHpModifierSystem`)
2. Create `NumericStatModifier` with unique ID
3. Call `.AddModifier()` during initialization

## Plugin Use

- Make sure to use plugin planning-with-files for all tasks.
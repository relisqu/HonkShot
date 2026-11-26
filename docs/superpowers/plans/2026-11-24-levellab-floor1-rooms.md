# LevelLab Floor-1 Rooms Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 30 new floor-1 room prefabs, built from JSON recipes, statically validated and bot-playtested against a baseline from existing floor-1 rooms.

**Architecture:** Editor-only tooling under `Assets/_Project/LevelLab/`: a recipe→prefab builder, a static validator, and a Play Mode bot that plays each room in isolation (scene reload per room, in-memory RunConfig with a single-room floor) through the real input path, including mid-flight re-throws.

**Tech Stack:** Unity 2D, C# (`#if UNITY_EDITOR`), Unity MCP (`create_script`, `execute_code`, `execute_menu_item`, `read_console`), JsonUtility.

**Spec:** `docs/superpowers/specs/2026-11-24-levellab-floor1-rooms-design.md`

## Global Constraints
- All new code in `Assets/_Project/LevelLab/`, namespace `Scripts.LevelLab`, whole files wrapped in `#if UNITY_EDITOR`. No edits to game scripts, game tests, `RunConfig.asset`, `FloorConfig_01/02.asset`.
- New .cs files are created via MCP `create_script` (Write-created scripts can skip import). `execute_code` = C# 6.
- Never git commit/push (user manages git). No "commit" steps.
- Rooms: variants of `Resources/Prefabs/Rooms/Room.prefab`, saved to `Resources/Prefabs/Rooms/LevelLab_Floor1/LL_<NN>_<Name>.prefab`.
- Forbidden in recipes: `SmallField`, `SmallWall*` (bounciness 0.5). `InnerWallObject` (0.9) max 1 per room.
- Floor-1 pieces only (`Enemies/StartFloor/*`, `LevelObjects/Bouncers/*` non-SecondFloor, `Arrow`, `Cannon`, `Hole`, `PointObject`, `MoveRoads/MoveRoad`, `InnerWallObject`).
- Targets: clear time within ±30% of baseline median (D3: up to +50%), 0 stuck events, sharp-drop rate ≤ baseline p75.
- Verification is via editor menus/console logs, not Unity Test Runner (user asked not to mix with tests).

## Review Focus
- Room outline drawn clockwise vs counter-clockwise → walls must still face inward (builder normalises winding to match existing rooms).
- Bot stuck in Swapping state if it starts a drag with 0 dashes → bot only calls DragStarted when `PlayerDashController.CanDash`.
- Time scale left at 0/slow after a scene reload → call `TimeManager.Instance.ResumeGame()` before every reload.
- Tutorial not completed in save → bot aborts with a clear log instead of hanging.
- A room the bot can't clear → hard timeout (90 s) recorded as `timeout`, run continues.

---

### Task 1: Recipe model, prefab catalog, builder

**Files:**
- Create: `Assets/_Project/LevelLab/Editor/RoomRecipe.cs`, `LevelLabCatalog.cs`, `LevelLabRoomBuilder.cs`
- Create: `Assets/_Project/LevelLab/Recipes/_sample.json`

**Interfaces:**
- Produces: `[Serializable] class RoomRecipe { string id; string name; string idea; int difficulty; Vector2[] outline; Vector2 spawn; Vector2 exit; RecipeObject[] objects; }`, `class RecipeObject { string key; Vector2 pos; float rot; Vector2 scale; }` (scale default 1,1).
- Produces: `LevelLabCatalog.TryGetPrefabPath(string key, out string path)`, `LevelLabCatalog.AllKeys`.
- Produces: `LevelLabRoomBuilder.Build(RoomRecipe r) -> string prefabPath`; menu `Tools/LevelLab/Build All Recipes`.

- [ ] Catalog keys: `SmallFly, BigFly, Shooter, LaserShooter, SpreadShooter, Follower, Walker, WalkerSpot, Bouncer, Bouncer1, Bouncer2, Bouncer3, Bouncer4, BouncerV, BouncerV1, SphereBouncer, Arrow, Cannon, Hole, PointObject, MoveRoad, InnerWall` → paths under `Resources/Prefabs/`.
- [ ] Builder: instantiate `Room.prefab` (InstantiatePrefab), set `Field` spline points (outline, coordinates are Field-local like existing rooms; keep Field at its prefab position), set `SpawnPoint`/`ExitPoint` local positions, clear existing `LevelObjects` children except `Walls`, InstantiatePrefab each object under `LevelObjects`, `SaveAsPrefabAsset` → variant, destroy instance, then invoke `RoomPrefabRefresher.RefreshRoomPrefab(path, out _)` via reflection to generate walls/corners.
- [ ] Verify: build `_sample.json` (rectangle + 3 SmallFly + 2 Bouncer); `execute_code` loads prefab: is variant of Room, Walls child count == outline points, 5 LevelObjects. Console has no errors.

### Task 2: Static validator

**Files:** Create `Assets/_Project/LevelLab/Editor/LevelLabValidator.cs`

**Interfaces:**
- Produces: `LevelLabValidator.Validate(GameObject roomPrefab) -> List<string> issues` (empty = pass); menu `Tools/LevelLab/Validate LevelLab Rooms` and `Tools/LevelLab/Validate Floor 1 Rooms` (writes `LevelLab/Reports/validation.txt`).

- [ ] Checks (world-space, Field transform applied): every LevelObjects child inside outline polygon; enemy ≥ 4.0 u from spawn; enemy not within 1.0 u of the spawn→nearest straight line? (warning only); no collider of placed objects overlapping wall colliders (Physics2D-free: use bounds vs wall OBB distance) or each other; pairwise solid-obstacle gap ≥ 2.1 u or 0 (touching wall counts as part of wall only if gap ≤ 0.05); interior outline angles ≥ 60°; min width: for each outline vertex, distance to non-adjacent edges ≥ 2.5 u; no BounceObject with bounciness < 0.85 among placed objects; polygon area within [min,max] of floor-1 rooms ±20%.
- [ ] Verify: run on floor-1 rooms → report produced; record which existing rooms fail which checks (calibration: thresholds that fail most existing good rooms are loosened and noted).

### Task 3: Playtest bot

**Files:** Create `Assets/_Project/LevelLab/Runtime/LevelLabSession.cs`, `LevelLabShotPlanner.cs`, `LevelLabBot.cs`, `LevelLabMetrics.cs`; `Assets/_Project/LevelLab/Editor/LevelLabPlaytestMenu.cs`

**Interfaces:**
- Produces: menu `Tools/LevelLab/Playtest LevelLab Rooms`, `Tools/LevelLab/Playtest Floor 1 Baseline`, `Tools/LevelLab/Playtest Room...` (single selected prefab). Results JSON → `LevelLab/Reports/<runName>.json`, summary in console `[LevelLab]`.
- `LevelLabSession` (DontDestroyOnLoad): queue of prefab paths from `SessionState`; on `sceneLoaded`: build in-memory `FloorConfigSO` (1 RoomModel, `_roomCount`=1, decorations/colors copied from FloorConfig_01) + `RunConfigSO`, inject into `LevelManager._runConfig` via reflection; ensure `DebugMode.Instance.GeneratingLevels`; attach `LevelLabBot`. On room complete/timeout: save metrics, `TimeManager.Instance.ResumeGame()`, reload scene, next room. End: exit Play Mode.
- `LevelLabShotPlanner.Plan(Vector2 origin, Vector2 currentVel, float throwSpeed, int samples=48) -> (Vector2 dir, float score)`: iterative `Physics2D.CircleCast` (radius = player ball collider radius, layer mask = collision mask of ball layer, ignore triggers), reflect on hit, multiply speed by hit `BounceObject.Bounciness`, apply the ball's linearDamping, stop below 1.5 u/s or 4 s; score = Σ alive enemies hit × speed-at-hit / (1 + t).
- `LevelLabBot`: drives `InputHandler.InputHandler_DragStarted()` → wait aim time 0.25 s unscaled → `InputHandler_DragFinished(-dir * dragMag)`. Throws when `CanDash` and (Idle/rest) or (Ball and (speed < 4 u/s or planner score ≥ 1.5× best of continuing current path)), min 0.6 s between throws. Keeps player alive: on HP < 50% restore full, count damage.
- `LevelLabMetrics`: per room `{room, cleared, time, throws, midFlightThrows, avgSpeed, sharpDrops (single-contact loss > 40% at speed > 6), stuckEvents (Ball, speed < 1 u/s for > 1.5 s with no dash, or > 8 contacts within 1 s inside 2 u radius), damageTaken, timeout}`.
- [ ] Verify: `Playtest Room...` on `New Rooms 2/Cross.prefab` and `New Rooms/Spiral.prefab` → both produce metrics with `cleared=true` (or timeout explained), no console exceptions.

### Task 4: Baseline
- [ ] Run `Playtest Floor 1 Baseline` (17 unique FloorConfig_01 rooms) ×2 → `Reports/baseline.json`; compute median/p25/p75 time, throws, sharpDrops/min. These numbers go into Task 5 briefs.

### Task 5: Room recipes (30)
- [ ] Dispatch 3 subagents, 10 recipes each (A: D1 rooms 01–10, B: D2 11–22, C: D3 23–30), given: spec, catalog keys + footprint sizes, existing room examples (outline + objects dump), research archetypes, baseline numbers, Global Constraints. Each writes `Recipes/LL_NN_<Name>.json` + one-line idea.

### Task 6: Build + validate
- [ ] `Build All Recipes`, `Validate LevelLab Rooms`; fix recipes until validation passes (or warnings justified).

### Task 7: Bot playtest + iterate
- [ ] `Playtest LevelLab Rooms`; for each room off-target: adjust recipe (enemy count/hp-mix, geometry, bouncers), rebuild, re-test. Max 3 iterations per room; remaining issues noted in report.

### Task 8: Deliver
- [ ] Create `Resources/Data/Floors/FloorConfig_LevelLab.asset` (flat, 30 rooms, `_roomCount` 6, decorations/colors of FloorConfig_01) via editor code.
- [ ] Write `Assets/_Project/LevelLab/Reports/LevelLab_Floor1_Report.md` (per room: idea, difficulty, bot metrics vs baseline, changes made) and how-to-playtest instructions.

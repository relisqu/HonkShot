# Progress Log

## Session: 2026-01-31

### Phase 1: DebugMode — Safe item spawning
- **Status:** complete
- Actions taken:
  - Read `DebugMode.cs` — found `SpawnItemWithIndex()` using `.First()` (throws on miss)
  - Replaced with `FirstOrDefault` + null check
  - Added `Debug.LogError` for not-found case, `Debug.Log` for success
- Files modified:
  - `Assets/_Project/Scripts/Other/DebugMode.cs` (edited)

### Phase 2: ItemPoolSO — Pool ScriptableObject
- **Status:** complete
- Actions taken:
  - Explored ItemManager, PlayerItemSO, PlayerInventory, StagePoolSO via codebase search
  - Designed ItemPoolSO following existing StagePoolSO pattern
  - Created `ItemPoolSO.cs` with `List<PlayerItemSO> Items` and `[Button] LoadAllItems()`
  - Modified `ItemManager.cs`: added `itemPool` field, populate `allItems` from pool in Awake, removed old `LoadItems()` button
- Files created:
  - `Assets/_Project/Scripts/Items/PlayerItemManager/ItemPoolSO.cs` (new)
- Files modified:
  - `Assets/_Project/Scripts/Items/PlayerItemManager/ItemManager.cs` (edited)

### Debug kill hotkey (F9)
- **Status:** complete
- Actions taken:
  - Added `DebugEnabled` bool field to `DebugMode`
  - Added `Update()` that checks F9 key when debug is on, deals 1000000 damage via player's `HealthController`
  - Gets HealthController from `PlayerInventory.Instance` (same GameObject as player)
- Files modified:
  - `Assets/_Project/Scripts/Other/DebugMode.cs` (edited)

### ExtraPickSystem — recalculate per pick, not per screen
- **Status:** complete
- Actions taken:
  - **ItemSelectionUI.cs**: `OnItemSelected` now calls `CalculateExtraPicks()` after each item pick, then `TryUseExtraPick()`. This gives a fresh chance roll every pick instead of once per selection screen.
  - **ExtraPickSystem.cs**: Reverted `_calculatedProviders` tracking (was wrong fix). Restored simple `Register/Unregister/CalculateExtraPicks`.
  - **LuckyCardItem.cs**: Kept `InitItem()` registration (harmless, good practice).
- Root cause: `CalculateExtraPicks()` was only called once in `ShowItems()`. Extra picks could only trigger on the first pick of a selection screen.
- Files modified:
  - `Assets/_Project/Scripts/UI/ItemSelectionUI.cs` (edited)
  - `Assets/_Project/Scripts/Items/ExtraPickSystem.cs` (edited)

### Rename OnDamaged → OnNonLethalDamageReceived(float)
- **Status:** complete
- Actions taken:
  - Renamed `OnDamaged` to `OnNonLethalDamageReceived` in HealthController
  - Changed type from `Action` to `Action<float>`, passes `finalDamage`
  - Updated all 7 subscriber files: handler methods now accept `float damage` parameter
- Files modified:
  - `Scripts/Health/HealthController.cs`
  - `Scripts/Health/InvincibilityController.cs`
  - `Scripts/Enemies/Bosses/RavageBossEnemy.cs`
  - `Scripts/Enemies/EnemyHealthBar.cs`
  - `Scripts/Enemies/EnemyHealth.cs`
  - `Scripts/Enemies/PlayerHealth.cs`
  - `Scripts/Items/PermanentItems/VampiricItem.cs`
  - `Scripts/Other/DamageBlinkModule.cs`

### RoomEntryFireItem — new item
- **Status:** complete
- Actions taken:
  - Created `RoomEntryFireItem.cs` — subscribes to `LevelManager.EnteredRoom`, adds `_fireAmount` fire on each room entry
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/RoomEntryFireItem.cs` (new)

### PassiveFireRegenItem — new item
- **Status:** complete
- Actions taken:
  - Created `PassiveFireRegenItem.cs` — adds `_fireAmount` fire every `_interval` seconds when fire is below `_fireThreshold`
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/PassiveFireRegenItem.cs` (new)

### DamageToFireConvertorItem — new item
- **Status:** complete
- Actions taken:
  - Added `Action<float> OnDamageReceived` to `HealthController`, fires with `finalDamage` when health is actually reduced
  - Added `public void AddFire(float)` to `GooseFireSystem` (wraps private `ChangeFire`)
  - Created `DamageToFireConvertorItem.cs` — subscribes to `OnDamageReceived`, converts damage to fire via `GooseFireSystem.AddFire(damage * coefficient)`
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/DamageToFireConvertorItem.cs` (new)
- Files modified:
  - `Assets/_Project/Scripts/Health/HealthController.cs` (added OnDamageReceived)
  - `Assets/_Project/Scripts/Player/GooseFireSystem.cs` (added AddFire)

### HonkHpConverterItem — new item
- **Status:** complete
- Actions taken:
  - Added `Action<float> OnDamageBlocked` event to `HealthController`, fires inside invincibility check with raw damage amount
  - Created `HonkHpConverterItem.cs` — subscribes to `OnDamageBlocked`, checks `GooseFireSystem.Instance.IsHonk`, applies cooldown, heals `damageAmount * coefficient`
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/HonkHpConverterItem.cs` (new)
- Files modified:
  - `Assets/_Project/Scripts/Health/HealthController.cs` (edited — added OnDamageBlocked)

### Select SO button on Item prefab
- **Status:** complete
- Actions taken:
  - Added `[Button("Select SO")]` to `Item.cs` that finds matching `PlayerItemSO` by name and pings + selects it
  - Uses same name-matching logic as existing `LinkToSO` button
- Files modified:
  - `Assets/_Project/Scripts/Items/PlayerItemManager/Item.cs` (edited)

### Phase 3: Verification
- **Status:** pending (requires Unity editor)
- Steps for user:
  1. Create ItemPool asset (Create > ScriptableObjects > ItemPool)
  2. Click "Load All Items" on it
  3. Assign to `ItemManager.itemPool` in scene
  4. Play mode test

---

## Session: 2026-02-01

### Explosion System — core
- **Status:** complete
- Actions taken:
  - Created `ExplosionSystem.cs` — singleton on player, 3 `NumericStatModifierSystem` (damage, radius, knockback), `CreateExplosion()` overloads, `OnExplosionCreated` event
  - Created `Explosion.cs` — spawnable explosion object, `Physics2D.OverlapCircleAll` for instant AOE damage + knockback via `EnemyHealth`/`Rigidbody2D`, scales VFX, self-destructs
- Files created:
  - `Assets/_Project/Scripts/Player/ExplosionSystem.cs` (new)
  - `Assets/_Project/Scripts/Player/Explosion.cs` (new)

### Player events — OnDashUsed, OnBounced
- **Status:** complete
- Actions taken:
  - Added `public Action OnDashUsed` to `PlayerDashController`, fires in `DecreaseDashCount()`
  - Added `public Action<Collision2D> OnBounced` to `PlayerBallMovement`, fires in `OnCollisionEnter2D()`
- Files modified:
  - `Assets/_Project/Scripts/Player/Dash/PlayerDashController.cs` (edited)
  - `Assets/_Project/Scripts/Player/PlayerBallMovement.cs` (edited)

### StickyMine refactor
- **Status:** complete
- Actions taken:
  - Replaced direct `HealthController.TakeDamage()` with `ExplosionSystem.Instance.CreateExplosion()`
  - StickyMine now benefits from all explosion modifiers (damage, radius, knockback)
  - Removed unused `_enemy` field and `HealthController`/`UIFactory` references
- Files modified:
  - `Assets/_Project/Scripts/Items/PermanentItems/StickyMine.cs` (edited)

### PirotechnicItem — radius buff
- **Status:** complete
- Actions taken:
  - Created `PirotechnicItem.cs` — adds `NumericStatModifier(Mult)` to `ExplosionSystem.RadiusModifierSystem`
  - Uses `playerItemSO.NumericId` for modifier ID (same pattern as DamageBuffItem)
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/PirotechnicItem.cs` (new)

### DashExplosionItem — explosion on dash
- **Status:** complete
- Actions taken:
  - Created `DashExplosionItem.cs` — subscribes to `PlayerDashController.OnDashUsed`, creates explosion at player position
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/DashExplosionItem.cs` (new)

### UnstableItem — explosion on bounce with chance
- **Status:** complete
- Actions taken:
  - Created `UnstableItem.cs` — subscribes to `PlayerBallMovement.OnBounced`, `_explosionChance` (30%) check, explosion at contact point
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/UnstableItem.cs` (new)

### FireworksItem — periodic explosion
- **Status:** complete
- Actions taken:
  - Created `FireworksItem.cs` — explosion around player every `_cooldown` seconds
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/FireworksItem.cs` (new)

### BigBombItem — explosion on ult
- **Status:** complete
- Actions taken:
  - Created `BigBombItem.cs` — subscribes to `GooseFireSystem.UltimateStarted`, massive explosion with custom damage/radius/knockback + cooldown
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/BigBombItem.cs` (new)

### PiromaniacItem — explosions add fire
- **Status:** complete
- Actions taken:
  - Created `PiromaniacItem.cs` — subscribes to `ExplosionSystem.OnExplosionCreated`, adds `_fireAmount` fire per explosion
- Files created:
  - `Assets/_Project/Scripts/Items/PermanentItems/CustomItems/PiromaniacItem.cs` (new)

### Verification (requires Unity editor)
- **Status:** pending
- Steps for user:
  1. Create `Explosion` prefab (empty GameObject with `Explosion` component)
  2. Add `ExplosionSystem` component to player prefab/GameObject
  3. Assign `_explosionPrefab` and configure `_enemyLayerMask`, default values
  4. Create 7 PlayerItemSO assets + prefabs for each new item
  5. Add all to ItemPool, test via DebugMode

## 5-Question Reboot Check
| Question | Answer |
|----------|--------|
| Where am I? | Explosion system implementation complete |
| Where am I going? | User verification in Unity editor |
| What's the goal? | Explosion system with modifier systems + 7 explosive items |
| What have I learned? | See findings.md — explosion architecture, player events, modifier patterns |
| What have I done? | See session log above — all code tasks complete |

---

## Session: 2026-08-08

### Aim-Zoom Feature — punchy overshoot on throw release
- **Status:** complete
- **Key discovery:** the live aim-zoom (camera eases out while aiming, scaled by drag power) was
  ALREADY implemented in `PlayerSwapAnimationHandler.HandleCameraZoom()` — the saved memory plan
  (new `CameraAimZoom` component) was outdated. A new component would fight the existing one over
  `Lens.OrthographicSize`. User confirmed: extend the existing handler.
- Actions taken:
  - Added serialized "Throw Release Snap" params: `_snapDuration=0.25f`, `_snapEase=Ease.OutBack`, `_snapOvershoot=2f`
  - `ResetCameraZoom()` → `ResetCameraZoom(bool punchy)`: throw release uses `_snapEase`+`_snapOvershoot` (punchy overshoot back past base then settle); cancel keeps clean `Ease.OutQuad`
  - `PlayerMovement_DragFinished` → `punchy:true`; `PlayerMovement_InputCancelled` → `punchy:false`
  - Recompiled via MCP, 0 console errors
- Files modified:
  - `Assets/_Project/Scripts/Player/PlayerSwapAnimationHandler.cs`
- Note: new fields have C# initializer defaults → existing player instances work with no MCP wiring.

### Aim-Zoom Feature — pivot to shake punch (pixel-perfect blocked overshoot)
- **Status:** complete (play-mode confirm pending)
- User reported the ortho overshoot had "no effect". Investigated via Unity MCP execute_code:
  - Scene camera: `Virtual Camera` GO has `CinemachinePixelPerfect`; `Main Camera` has URP
    `PixelPerfectCamera` (assetsPPU=26, refRes 960x540, gridSnapping=UpscaleRenderTexture, pixelRatio=4).
  - Pixel-perfect quantizes lens ortho size → sub-unit overshoot near base is invisible.
  - Verified serialized field values were correct (0.25/OutBack/2), so NOT a Unity defaults bug.
  - User confirmed zoom-OUT looks smooth (many fine steps) but chose shake-punch approach.
- Actions taken:
  - Removed `_snapDuration/_snapEase/_snapOvershoot`; reverted `ResetCameraZoom()` to single OutQuad (throw+cancel)
  - Added `_throwShakeDuration=0.15f`, `_throwShakeStrength=3f` ("Throw Release Punch" header)
  - `PlayerMovement_DragFinished` → `CameraShakeHandler.Instance.ShakeCameraOutsideOfQueue(dur, str)` (throw only)
  - Recompiled 0 errors; verified instance fields 0.15 / 3
- Files modified:
  - `Assets/_Project/Scripts/Player/PlayerSwapAnimationHandler.cs`

### Aim-Zoom Feature — FINAL: bypass pixel-perfect during aim (real zoom + overshoot)
- **Status:** implemented, compiles clean; NOT playtested (MCP play sessions don't tick frames)
- User wanted an actual zoom while keeping pixel-perfect "best effort". Proven via play-mode MCP tests:
  - URP PixelPerfectCamera (UpscaleRenderTexture) hard-locks rendered ortho to 10.38461; Cinemachine
    lens is ignored; vcam.State.Lens doesn't follow vcam.Lens.
  - Direct `Camera.orthographicSize` writes hold ONLY when BOTH URP PixelPerfectCamera AND
    CinemachineBrain are disabled.
  - Crisp pixel-perfect zoom steps are coarse (10.38→13.85→20.77) — too jarring.
- Approach: on aim start, disable brain + URP PixelPerfect + Cinemachine PixelPerfect, capture crisp base,
  drive `_outputCamera.orthographicSize` directly (zoom-out scales with drag). On throw release, tween back
  with OutBack overshoot (the punch); on cancel, clean OutQuad. Re-enable all on tween complete. Both
  tweens `SetUpdate(true)`; Update() safety-net prevents a stuck-disabled state.
- Serialized refs `_urpPixelPerfect` (Main Camera) + `_cmPixelPerfect` (Virtual Camera) wired via MCP; scene saved.
- Dropped the earlier shake-punch (overshoot now works as the punch).
- INCIDENT: an earlier play-mode experiment leaked PixelSnapping/upscaleRT=false + cm.disabled into the
  saved scene (MCP play-state tracking returned "already stopped" while still playing → SaveScene captured
  runtime values). Restored to UpscaleRenderTexture/upscaleRT=true/cm.enabled=true and re-saved. Lesson:
  verify Application.isPlaying==false before SaveScene.
- Files modified:
  - `Assets/_Project/Scripts/Player/PlayerSwapAnimationHandler.cs`

---

## Session: 2026-08-12

### Aim Pan — camera pans toward throw direction while aiming, snaps back on release
- **Status:** implemented, NOT compile-verified (Unity editor not running) or playtested
- Feature: extend the existing aim-zoom in `PlayerSwapAnimationHandler` so the camera also pans a bit
  toward the throw direction while charging, then snaps back on release. Rides the pixel-perfect bypass
  that's already open during aim (brain disabled → output camera position is ours to drive).
- Approved design choices: pan scales with drag power (reuses the zoom's `forceScale`); punchy OutBack
  snap-back to center on throw; clean OutQuad on cancel (mirrors the zoom's punchy/clean split).
- Throw direction = `-GetCurrentDrag().normalized` (launch goes opposite the drag — see
  `PlayerBallMovement.ThrowRigidbody` line 137: `linearVelocity = -currentVelocity * dragForce.normalized`).
- Actions taken:
  - Added serialized `_maxPanOffset = 1.5f` (world units at full pull), grouped under the Aim Zoom header
  - Added `_originalCameraPos` + `_panTween`; capture base pos in `BeginPixelPerfectBypass`
  - New `HandleCameraPan()` called alongside `HandleCameraZoom()` in `SetDragAnimation()` — per-frame
    DOMove of `_outputCamera.transform` toward `base + throwDir * forceScale * _maxPanOffset` (z preserved),
    short `_zoomDuration` OutQuad, killed/restarted like the zoom tween
  - `ResetCameraZoom(bool punchy)` now also tweens position back to `_originalCameraPos` (punchy → OutBack
    + `_snapOvershoot`; cancel → OutQuad)
  - Added `TryEndPixelPerfectBypass()` — re-pins pixel-perfect only after BOTH zoom and pan tweens finish
    (else the re-enabled brain yanks the camera and cuts the overshoot); `Update()` safety-net also checks
    `_panTween`
- Files modified:
  - `Assets/_Project/Scripts/Player/PlayerSwapAnimationHandler.cs`
- TODO (user): open Unity to recompile + playtest; tune `_maxPanOffset` on the player instance.

### Aim Camera — extracted to AimCameraController component (single facade)
- **Status:** code done, NOT compile-verified (MCP bridge showed 0 instances despite Unity running)
- Refactor: all aim-camera logic (pixel-perfect bypass + zoom + pan + snap-back) moved OUT of
  `PlayerSwapAnimationHandler` into a new `Scripts/Camera/AimCameraController.cs` singleton. Chosen
  structure (user): single facade component, not pluggable effects.
- Public API the handler now calls: `BeginAim()`, `UpdateAim(Vector2 dragForce, float forceScale)`,
  `ReleaseAim(bool punchy)`. Handler holds zero camera state.
- `AimCameraController` auto-resolves refs in Start: `_virtualCamera` from `CameraShakeHandler.Instance`,
  `_cmPixelPerfect` from the vcam, `_urpPixelPerfect`/`_outputCamera`/`_brain` from `Camera.main`.
  `_urpPixelPerfect` + `_cmPixelPerfect` are optional serialized overrides if auto-resolve misses.
- Serialized tunables moved to the new component: `_maxZoomOut` (1), `_zoomDuration` (0.1),
  `_maxPanOffset` (1.5), `_snapDuration` (0.25), `_snapEase` (OutBack), `_snapOvershoot` (2).
- Dropped dead fields from handler: `_minSize`, `_maxZoomAddOut` (both declared, never used).
- Files created: `Assets/_Project/Scripts/Camera/AimCameraController.cs`
- Files modified: `Assets/_Project/Scripts/Player/PlayerSwapAnimationHandler.cs`
- TODO (Unity, once MCP bridge connects or user does it manually):
  1. Add `AimCameraController` component to the `CameraShakeHandler` GameObject (so its singleton exists).
  2. Migrate any values the user had TUNED on the old handler instance (zoom/snap/pan) onto the new
     component — couldn't read them, bridge was down. If untuned, the C# defaults above match the old ones.
  3. If `Camera.main` isn't the pixel-perfect output camera, wire `_urpPixelPerfect` explicitly.
  4. Recompile (check console) + playtest.
- DONE + verified in Unity (2026-08-13): component added to `Virtual Camera` GO, auto-resolve confirmed
  (Camera.main = Main Camera has PixelPerfectCamera+CinemachineBrain; vcam has CinemachinePixelPerfect),
  compiled 0 errors, scene saved. Defaults on instance: maxZoomOut 1 / zoomDuration 0.1 / maxPanOffset 1.5
  / snapDuration 0.25 / OutBack / overshoot 2.

---

## Session: 2026-08-13

### Trajectory prediction FPS drop during aim — diagnosis + optimization
- **Root cause (100% confirmed by user via Unity Profiler):** `Projection.Update` → `SimulateTrajectory` →
  `Physics2D.Simulate`. Two per-frame costs:
  1. Recalc every frame while aiming: `_needsRecalculation` was set true every frame the drag changed and
     BYPASSED the `_simulationRefreshInterval` throttle → simulation ran every frame (up to 144Hz), and our
     new aim-pan means the aim/camera moves constantly → constant recalc.
  2. Each recalc = up to `_maxPhysicsFrameIterations` physics `Simulate` calls. **Instance values on the
     `GameManager` Projection were 500 iters / simSubSteps 5 / maxBounces 3 / refreshInterval 0.03** (NOT the
     code defaults 100/4/2/0.05) — so up to 500 Simulate calls per recalc.
- NOTE: my earlier "16x render at 4K because pixel-perfect bypass disables PixelPerfectCamera" hypothesis was
  WRONG — profiler showed CPU (Scripts/Physics2D), not GPU. Screen was 3840x2160 vs 960x540 RT, but that
  wasn't the cause. A/B toggle `_bypassUrpPixelPerfect` was added then fully reverted.

- **FIX 1 (DONE, compiled clean): throttle.** `Projection.Update` now simulates only when
  `Time.unscaledTime >= _nextSimTime` while dragging (removed the per-frame `_needsRecalculation` bypass);
  `OnDragStarted` sets `_nextSimTime = 0` for an immediate first sim. Ghost sync moved out of per-frame into
  new `SyncGhosts()` called only right before each simulation. Code default `_simulationRefreshInterval`
  0.05→0.033 (30Hz), but instance already 0.03 so unchanged. User confirmed this helped.
  Files: `Assets/_Project/Scripts/Player/ThrowTrajectory/ProjectilePrediction.cs`

- **FIX 2 (CODE WRITTEN, NOT YET COMPILED OR PLAYTESTED): adaptive stepping.** User wanted each calc cheaper
  without shortening the line. Reducing `_simSubSteps` (5→2/3) made bounces off MOVING objects noticeably
  worse (substeps = bounce-accuracy knob), so instead implemented adaptive stepping in `SimulateTrajectory`:
  per step, `_physicsScene.CircleCast` looks ahead `coarseStep + _wallProximityMargin`; if a wall is close →
  fine step (`_bounceSubSteps=8`), else coarse step (`_flightSubSteps=1`). Self-hit filtered via
  `hitCol != _ghostCollider`; cast filter = SimulationGhost layer. Loop now caps by total sim TIME
  (`maxIter * fixedDt / simSubSteps`) so line length is preserved; `_simSubSteps`/`_maxPhysicsFrameIterations`
  now only define length + point cap, not resolution.
  - New serialized fields (auto-default on instance): `_flightSubSteps=1`, `_bounceSubSteps=8`,
    `_wallProximityMargin=0.75`.
  - Verified `PhysicsScene2D.CircleCast` signature via unity_reflect and fixed arg order to
    `(origin, radius, direction, distance, contactFilter, results)`.
  - **PENDING (blocked): user was in Play Mode so it hasn't recompiled.** NEXT: user exits Play → I read
    console for compile errors → user re-enters Play and checks: moving-object bounce accuracy, FPS during
    aim, line length/smoothness. Tuning knobs if bounce slightly off: raise `_bounceSubSteps` and/or
    `_wallProximityMargin` (latter for fast throws).
  Files: `Assets/_Project/Scripts/Player/ThrowTrajectory/ProjectilePrediction.cs`
- Leftover cleanup (optional): after FIX 1, `_needsRecalculation` and `_dragChangeThreshold` in Projection are
  now unused/write-only. Also `SimulateFromCurrentState` was already dead. User offered removal; not done yet.

---

## Session 2026-08-21 — commit + FloorConfig "Refresh Rooms"

- **Commit `9fa0347`** on `dev`: "Extract aim camera logic and tune throw trajectory" — all pending
  edits (AimCameraController, ProjectilePrediction adaptive stepping, Wall.prefab, GameplayScene,
  package manifests, planning files, `Assets/_Recovery/0 (5).unity`). Not pushed.

- **Fixed `FloorConfigSO.RefreshRooms()`.** Root cause: generators were run straight on the prefab
  asset, so `CornerGenerator`'s `new GameObject` + `SetParent` landed in the open scene (the garbage)
  and nothing was written to the prefab; no save call either. See findings.md.
  - `FloorConfigSO`: added `RefreshRoomsRequested` static Action + `AllRoomPrefabs()` (default rooms,
    micropool rooms, boss rooms, deduped). Button now guards Play Mode / missing subscriber.
  - New `Assets/_Project/Scripts/Editor/RoomPrefabRefresher.cs`: `LoadPrefabContents` →
    `CopySpriteShapeSpline` + `GenerateWalls` on every `WallGeneration`, `Generate()` on every
    `CornerGenerator` → `SaveAsPrefabAsset` → `UnloadPrefabContents`. Cancelable progress bar,
    per-room try/catch, summary log.
  - `WallGeneration`: walls now spawn via `PrefabUtility.InstantiatePrefab` in edit mode (new
    `SpawnWall()`), so Wall.prefab edits propagate after one refresh. Null guards moved/added.
  Files: `FloorConfigSO.cs`, `Editor/RoomPrefabRefresher.cs`, `LevelObjects/WallGeneration.cs`

- **Verified:** Assembly-CSharp and Assembly-CSharp-Editor both compile with 0 errors using Unity's
  bundled Roslyn against the project's Bee .rsp files (Unity MCP was not connected this session).
- **PENDING (user):** let Unity recompile, hit "Refresh Rooms" on a FloorConfig, confirm no scene
  garbage and that room prefabs now hold Wall prefab instances. First run replaces the old
  disconnected wall copies — any hand-made tweaks on individual wall objects will be lost.

### Follow-up (same session): "walls still not prefabs"
- **Not a bug — it worked.** Verified against the YAML: before the run `Rectangle.prefab` had 4 plain
  GameObjects named `Wall`; it now has 4 nested `Wall.prefab` instances (guid
  `c8cca5adb4a3b194fb097120b92ae134`) and 0 plain ones. All 18 FloorConfig_01 rooms + the boss room
  variant converted (4–20 wall instances each, 0 plain).
- **Real gap found:** only `FloorConfig_01` was refreshed. `FloorConfig_02` rooms (`Rooms/SecondFloor/*`)
  still have plain walls, and only **6 of the 30** SecondFloor room prefabs are referenced by any floor
  config at all — the config button can never reach the other 24.
- **Added** `Assets/Rooms/Refresh Selected Room Prefabs` menu item to `RoomPrefabRefresher`: works on
  Project-window selection, expands selected folders via `FindAssets("t:Prefab", folders)`, confirmation
  dialog, and skips prefabs with no `WallGeneration`/`CornerGenerator` without saving them. Config
  refresh and selection refresh now share `RefreshPaths()`; counts are reported as
  refreshed / without-generators / failed.
- Verified: Assembly-CSharp-Editor compiles (Unity Roslyn, exit 0) and Unity's console is clean after
  a forced refresh+compile via MCP.

### Follow-up 2: RunConfig button + InnerWallGeneration
- `RunConfigSO`: added `RefreshRoomsRequested` static Action and a `RefreshAllRooms()` Odin button
  that walks every floor in the run, dedupes room paths and refreshes them in one pass.
- `InnerWallGeneration`: added `SpawnWall()` using `PrefabUtility.InstantiatePrefab` in edit mode
  (islands' inner walls now stay linked to `InnerWall.prefab`), plus `IsLocked` accessor.
- `WallGeneration`: added `IsLocked` accessor.
- `RoomPrefabRefresher`: now also drives `InnerWallGeneration` (`CopySpriteShapeSplineToHelper` +
  `GenerateWalls`, **not** `GenerateTrees` — see findings.md), respects `_locked` on both generator
  types and reports locked/no-generator/failed counts separately. Three entry points share
  `RefreshPaths()`: RunConfig button, FloorConfig button, Project-window menu item.
- Verified: both assemblies compile with Unity Roslyn (exit 0); Unity console clean after forced
  refresh+compile over MCP. **Not yet run in the Editor** — inner walls become added-GameObject
  overrides that are themselves prefab instances on a nested island instance; that is supported but
  worth eyeballing on the first run (Room_2_31 has the most, 2 islands / 16 inner walls).

---

## Session 2026-08-26 — cannon: stuck slow-mo + invisibility

Systematic debugging; two independent root causes, both confirmed by reading the call chain (see
findings.md).

- **Fix 1** `Assets/_Project/Scripts/Player/PlayerMovement.cs:39` — `-=` → `+=` on
  `_inputHandler.OnInputCancelled`. Restores every cancel consumer: time scale reset, character
  scale reset, aim camera release, sweat particles, and both trajectory lines.
- **Fix 2** `Assets/_Project/Scripts/LevelSystem/LevelObjects/CannonObject.cs` — recapture guard:
  `_lastShotObject` + `_lastShotTime` + serialized `_recaptureCooldown = 0.3f`, cleared in a new
  `OnTriggerExit2D`. Stops the cannon swallowing the player on the physics step right after the shot.
- **Fix 3** (same file) `OnDisable` releases a still-contained interactable. `HandleCannon` owns the
  player's visibility, invincibility and input; if the cannon's room is deactivated mid-cycle the
  coroutine dies and the player would stay invisible, invincible and input-locked forever.
- Also removed a leftover per-shot `Debug.Log(shootDirection)`.
- Verified: Assembly-CSharp compiles (Unity Roslyn, exit 0); Unity console clean after forced
  compile over MCP. **Not playtested** — no test assemblies exist in the project, so the behaviour
  check is manual: drag → enter cannon → confirm timescale returns to 1 immediately, aim line and
  camera reset, and the player is visible and stays out after the shot.

### Follow-up: cancel restores the pre-aim state
- `PlayerMovement.InputHandler_InputCancelled` now calls `_playerStatus.ResetSwapState()` instead of
  hard `SetPlayerState(PlayerState.Idle)`, so cancelling a mid-air aim leaves the player a ball.
  `AnimationStateMachine` already has the `SwappingToBall` / `SwappingToIdle` transitions, and
  `SetPlayerState(Ball)` fires `OnSwapToBall` → `PlaySwapToBallAnimation()`, so the visual swap is
  driven correctly either way.
- Guarded by `if (_isDragging)`: a press that never became an aim (`_playerDashController.CanDash`
  false, so `DragStarted` was never accepted) must not transform the player. The input handler still
  reports `IsDragging` on such a press, so without this guard cancel would restore a stale state.
- `PlayerStatus.SetPlayerState` no longer records `Swapping` as `_preSwapState` when it is already
  Swapping — otherwise a repeated `SetPlayerState(Swapping)` would make `ResetSwapState()` a no-op
  and strand the player mid-aim. (`ResetSwapState` had been dead code, so this latent bug never fired.)
- Note `PlayerState.Ball == 0`, so `_preSwapState` defaults to Ball before the first aim; harmless
  because cancel only restores when a Swapping transition actually happened.
- Verified: Assembly-CSharp compiles (exit 0), Unity console clean. Not playtested.

### Portal teleport drift
- Root cause: `PlayerBallMovement.FreezePhysics()` used Kinematic-only, which still lets
  `BouncingObject.OnCollisionEnter2D` assign `linearVelocity` — and kinematic bodies move when you
  assign velocity. Wall bounce during the swirl → shrunk player drifts against the `DOMove` tween.
- Fix: `FreezePhysics()` sets `simulated = false`; `UnfreezePhysics()` restores it from
  `_preFreezeSimulated` before re-zeroing velocities. Files: `PlayerBallMovement.cs`.
- Verified: Assembly-CSharp compiles (exit 0), Unity console clean. Not playtested — repro is
  "enter the exit portal while bouncing off a wall, watch the trail".
- Follow-up: player now genuinely enters a new level at rest. `BouncingObject.ResetVelocityTracking()`
  added and called from `UnfreezePhysics()` — the rigidbody was already zeroed, but the bounce
  tracker's `_currentVelocity` survived the teleport and would have been reused as `speed` by the
  first bounce in the new room. Compiles clean, Unity console clean.

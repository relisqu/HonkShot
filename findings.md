# Findings & Decisions

## Item System Architecture

### Key Files
| File | Path | Purpose |
|------|------|---------|
| ItemManager | `Scripts/Items/PlayerItemManager/ItemManager.cs` | Singleton, holds runtime item pool, provides random selection |
| PlayerItemSO | `Scripts/Items/PlayerItemManager/PlayerItemSO.cs` | ScriptableObject per item — Id, icon, ItemPrefab, Enabled flag |
| PlayerInventory | `Scripts/Items/PlayerItemManager/PlayerInventory.cs` | Tracks owned items, calls `OnPickup()` to instantiate |
| Item (abstract) | `Scripts/Items/PlayerItemManager/Item.cs` | Base MonoBehaviour for item logic, `InitItem(PlayerItemSO)` |
| DebugMode | `Scripts/Other/DebugMode.cs` | Debug tools — spawn items by Id or random |
| ItemPoolSO | `Scripts/Items/PlayerItemManager/ItemPoolSO.cs` | **NEW** — Pool SO with editor button to load all items |

### Item Flow
1. `ItemPoolSO` holds serialized list of all `PlayerItemSO` assets
2. `ItemManager.Awake()` reads pool, filters by `Enabled`, stores in `allItems`
3. `GetRandomItems(count)` picks from `allItems`, excluding already-owned
4. `PlayerInventory.AddItem(so)` → `so.OnPickup(player)` → instantiates prefab → `InitItem()`

### Item SO Fields
- `Id` (string) — unique identifier, e.g. "5", "22", "11-2"
- `NumericId` (int, computed) — parsed from Id for modifier system
- `icon` (Sprite) — UI display
- `ItemPrefab` (Item) — prefab with Item component
- `Enabled` (bool) — controls pool inclusion

### Existing Pool Pattern
`StagePoolSO` in LevelGeneration uses same pattern: SO with a list + `[CreateAssetMenu]`.

### Resources Location
Items stored at `Assets/_Project/Resources/Data/Items/` — loaded via `Resources.LoadAll<PlayerItemSO>("Data/Items")`

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| Pool SO uses `Resources.LoadAll` in editor button | Same proven loading path as old `LoadItems()` |
| `EditorUtility.SetDirty(this)` after load | Ensures Unity serializes the populated list |
| `#if UNITY_EDITOR` guard on button | `Resources.LoadAll` in editor context only; runtime reads serialized data |

## HealthController Events
- `OnTakeDamageTriggered` — fires after invincibility check passes, before damage calc
- `OnDamageBlocked(float)` — fires when damage is blocked by invincibility, passes raw damage amount
- `OnDamageReceived(float)` — fires when health is actually reduced, passes finalDamage (lethal + non-lethal)
- `OnNonLethalDamageReceived(float)` — fires after non-lethal damage applied, passes finalDamage (renamed from OnDamaged)
- `OnDied` — fires when health reaches 0 and revive fails
- `OnRevived` — fires when health reaches 0 but revive succeeds

## GooseFireSystem
- `AddFire(float amount)` — public method to add fire points externally
- `IsHonk` — public property, true while honk mode is active
- `Instance` — singleton access

## ExtraPickSystem Architecture
- `CalculateExtraPicks()` called per item pick in `ItemSelectionUI.OnItemSelected()` — gives fresh chance roll each pick
- `TryUseExtraPick()` called immediately after recalculation — if true, refreshes items for another pick
- With `ExtraPickChance = 0.25`, each pick independently has 25% chance to trigger another pick (converges naturally)

## ExplosionSystem Architecture
- `ExplosionSystem` — Singleton on player, has 3 `NumericStatModifierSystem` (damage, radius, knockback)
- `Explosion` — Spawned object, does `Physics2D.OverlapCircleAll` for instant AOE damage + knockback, plays VFX, self-destructs
- `OnExplosionCreated(Vector2, float)` — Event fired after every explosion, used by PiromaniacItem
- Items use `ExplosionSystem.Instance.CreateExplosion()` overloads (position only, with baseDamage, or full 3-param)
- Modifier items (PirotechnicItem) add `NumericStatModifier` to the system, affecting all explosions globally

## Player Events (new)
- `PlayerDashController.OnDashUsed` — fires in `DecreaseDashCount()` after dash consumed
- `PlayerBallMovement.OnBounced` — fires in `OnCollisionEnter2D()` on every collision while in Ball state

## Code Style Reminders
- Always encapsulate fields: private `_camelCase` backing field + public `PascalCase` property
- Never expose raw fields as public when a property will do
- Example: `private List<T> _allItems;` + `public List<T> AllItems => _allItems;`

## Boss Animation System (2026-06-05)

### Boss Phase Code Map
- `RavageBossEnemy._currentPhase == 0` — minions rotating, boss invincible, follows player, shoots single
- `_currentPhase == 1` (set by `GoToSecondPhase`) — minions dead, shields up, boss vulnerable, still follows and shoots
- `_currentPhase == 2` (set by `GoToThirdPhase`) — shields broken, bezier waypoint movement, dashing, single/spread pattern
- User-facing naming is 1-indexed: their "phase 1" = code phase 0, their "phase 2" = code phase 1

### Boss Visual Architecture
- `Boss.prefab` hierarchy: `EnemyTransform` → `Visual` → `Circle`
- `BossVFXController._visualTransform` points at `Visual` — used for procedural DOTween scale (shot squash/stretch)
- `BossVFXController._spriteRenderer` points at `Circle`'s SpriteRenderer — used for the damage flash
- Existing damage flash uses `_spriteRenderer.color = Color.black` for `_flashDuration` then restores `_originalColor`
- No Animator exists on the prefab currently; all visual feedback is procedural

### Material Swap Pattern (reused)
- `Assets/_Project/Scripts/LevelSystem/LevelObjects/PunchObjectAnimation.cs` is the reference implementation:
  1. Cache `previousMaterial = _spriteRenderer.material`
  2. `_spriteRenderer.material = _blinkColorMaterial`
  3. Run animation (DOTween)
  4. On complete: restore `previousMaterial`
- Same pattern is being applied to `BossVFXController.FlashHit`
- The actual blink material asset reference: TBD — locate the one the bouncer prefab uses, reuse it for the boss

### Boss Coroutine Gating
- `FollowPlayerCoroutine` and `ShootingCoroutine` both loop on `while (_currentPhase < 2)` — they write velocity / fire shots every frame
- To pause them mid-phase (e.g., during phase-transition shake), need a separate `_isPaused` flag; stopping/restarting the coroutines would lose state and is unnecessary
- `RotateMinions` loops on `_currentPhase == 0` — naturally stops when `GoToSecondPhase` flips the phase, so no special handling needed during transition pause (minions are already dead by then anyway)

### Spec Location
- `docs/superpowers/specs/2026-06-05-boss-animation-system-design.md`

## Aim-Zoom vs Pixel-Perfect Conflict (2026-08-08) — ROOT CAUSE

Aim-zoom overshoot had "no effect". Root cause is NOT the tween code — it's pixel-perfect quantization.

Scene camera setup:
- `Virtual Camera` GO (instanceID 57948) has `Unity.Cinemachine.CinemachinePixelPerfect` extension (enabled).
- `Main Camera` has URP `PixelPerfectCamera` (enabled): assetsPPU=26, refResolution=960x540,
  gridSnapping=**UpscaleRenderTexture**, upscaleRT=true, pixelRatio=4.

Mechanism:
- In UpscaleRenderTexture mode the scene renders to a fixed-resolution RT, then integer-upscales to screen.
- `CinemachinePixelPerfect` snaps the vcam `Lens.OrthographicSize` to discrete integer-ratio zoom levels
  every frame (Finalize pipeline stage), overriding any tween.
- Current camera size = 10.38461 = 540/(2*26) = the reference pixel-perfect size → pinned to base level.
- => Any sub-step lens change (the fractional overshoot) is invisible. The existing aim zoom-OUT is
  quantized/overridden too.

Implication: smooth continuous ortho zoom is fundamentally incompatible with this strict pixel-perfect config.
Fix options (need user decision — architecture-level, see task_plan.md):
  1. Deliver the "punch" via a channel pixel-perfect doesn't quantize: camera impulse/shake kick on release
     (CameraShakeHandler.ShakeCamera already exists) instead of ortho overshoot.
  2. Temporarily disable CinemachinePixelPerfect (+maybe PixelPerfectCamera) during aim+snap, re-enable on
     settle — smooth zoom but brief non-pixel-perfect rendering / possible pop.
  3. Accept that ortho-zoom feel can't work here; drop the overshoot.

## Room prefab regeneration (2026-08-21)

- **Unity forbids parenting new transforms into a Prefab Asset.** Calling a generator that does
  `new GameObject(...)` + `SetParent(...)`, or `Instantiate(prefab, parent)`, directly on
  `somePrefab.GetComponent<T>()` creates the objects in the **currently open scene** instead.
  That is exactly what `FloorConfigSO.RefreshRooms()` was doing — hence "garbage in the scene,
  nothing spawned in the prefab".
- The supported route is `PrefabUtility.LoadPrefabContents(path)` → mutate the returned root
  (it lives in a temporary preview scene where parenting is legal) → `PrefabUtility.SaveAsPrefabAsset`
  → `PrefabUtility.UnloadPrefabContents` in a `finally`.
- `[ExecuteInEditMode] Update()` does **not** tick inside a prefab-contents preview scene, so
  `WallGeneration`'s once-per-second auto-regeneration cannot interfere with the refresh.
- `WallGeneration` spawned walls with plain `Instantiate`, producing disconnected copies. Using
  `PrefabUtility.InstantiatePrefab` in edit mode keeps the nested prefab link, so after one refresh
  Wall.prefab edits propagate to all rooms automatically.
- `_Project` has **no asmdef** (only third-party ones), so it is Assembly-CSharp /
  Assembly-CSharp-Editor. Runtime code cannot call editor code; the codebase already solves this
  with a static `Action` on the SO plus an `[InitializeOnLoad]` listener (`FloorPreviewer`).

### Offline C# compile check without Unity (no MCP, no .NET SDK)
Unity's own compiler + the project's real reference set can be driven from the shell:

```
UNITY=/c/Program\ Files/Unity/Hub/Editor/6000.2.10f1/Editor/Data
D=Library/Bee/artifacts/1900b0aE.dag        # E = editor dag, P = player dag
# copy $D/Assembly-CSharp.rsp, rewrite -out:/-refout: to a scratch path, then:
"$UNITY/NetCoreRuntime/dotnet.exe" "$UNITY/DotNetSdkRoslyn/csc.dll" @scratch.rsp
```
For the editor assembly also rewrite its `-r:".../Assembly-CSharp.ref.dll"` to the freshly built
ref assembly, and append any new source files (the .rsp lists files explicitly).
`info USG0001: Only allowed to have one file passed in with extension '.AdditionalFile.txt'` is a
harmless source-generator info, not an error.

### InnerWallGeneration (islands) — separate class, was never refreshed
- `InnerWallGeneration` does **not** derive from `WallGeneration`, so
  `GetComponentsInChildren<WallGeneration>` never found it. Islands were completely untouched by the
  first version of the refresher.
- It has the same two defects the outer walls had: `GenerateWalls()` used plain `Instantiate` (walls
  disconnected from `InnerWall.prefab`), and `GenerateTrees()` does `new GameObject` + `SetParent`,
  which is exactly the operation Unity forbids on a prefab asset.
- Wiring: `Rooms/SecondFloor/*` rooms contain nested `LevelObjects/InnerWallObject.prefab` instances
  (the islands); that prefab holds the `InnerWallGeneration` component and points `_wallPrefab` at
  `Resources/Prefabs/InnerWall.prefab` (guid `db5cd1edc2f28bc4db199ed9f2e1538b`). The generated inner
  walls live in the room as *added GameObject* overrides on the nested island instance, so they must
  be regenerated per room, not once in `InnerWallObject.prefab`.
- 8 room prefabs contain islands: Room_2_4, _11, _12, _23, _24, _27, _29, _31 (1–2 islands each,
  4–16 inner walls each).
- **Trees are deliberately NOT regenerated by the refresher.** They are random, and at runtime
  `LevelManager.FloorEntered` rebuilds them from `FloorConfig.Decorations` — baking floor-specific
  trees into a room prefab that several floors can share would be wrong and would add random churn
  to every save.

### Config coverage gaps (as of 2026-08-21)
- `RunConfig.asset` lists **only FloorConfig_01**. FloorConfig_02 is not part of the run, so the
  run-level button does not reach it.
- `FloorConfig_02` lists only 6 of the 30 `Rooms/SecondFloor/*` prefabs.
- Net: config-driven buttons reach 18 rooms; the other ~24 SecondFloor rooms are only reachable via
  the `Assets/Rooms/Refresh Selected Room Prefabs` menu item.
- No room prefab currently has `_locked: 1` on any generator.

## Cannon bugs (2026-08-26)

### RC1 — `PlayerMovement.OnEnable` unsubscribed instead of subscribing
```csharp
_inputHandler.OnInputCancelled -= InputHandler_InputCancelled;   // was -=, must be +=
```
`PlayerMovement.InputCancelled` therefore **never fired**, so every cancel consumer was dead code:
- `PlayerSwapAnimationHandler.PlayerMovement_InputCancelled` → no `ResetTimeScale()` (timeScale stuck
  at 0.1), no `ResetCharacterScale()`, no `AimCameraController.ReleaseAim()`, no `StopSweatEmission()`
- `ProjectilePrediction` / `RaycastTrajectory` → aim lines never cleared on cancel
- `_isDragging` stayed true and `PlayerState` stayed `Swapping`

Chain for the reported bug: entering the cannon calls `InputHandler.SetInputEnabled(Cannon, false)`,
which calls `CancelInput()` → `MouseInputHandler.CancelInput()` → `OnCancelInput` → InputHandler
re-raises `OnInputCancelled` (no `IsInputEnabled` guard, so it does fire) → **nobody listening**.
Slow-mo persisted until the player released the mouse *after* the shot, at which point
`InputHandler_OnDragFinished` finally ran `ResetTimeScale()` — matching "0.1 for a long time until
cannon is shot".

Amplifier: `yield return new WaitForSeconds(pauseTime)` in `Cannon.HandleCannon` is timeScale-scaled,
so the 0.5 s barrel pause became 5 real seconds at timeScale 0.1. Left as-is on purpose — with RC1
fixed the timescale is back to 1 before the wait matters, and changing it would alter game feel.

### RC2 — cannon recaptured the player it had just shot
`PlayerCannonInteractable.OnEnterCannon` sets root `localScale = Vector3.zero` (that is the
"invisible"), `OnExitCannon` sets it back to `Vector3.one`. `OnExitCannon` also re-enables the
player's collider **while it is still inside the cannon's trigger**. Unity raises `OnTriggerEnter2D`
again on the next physics step, and by then `HandleCannon` has already cleared `_containedObject`
(same frame, synchronously) — so the cannon captured the player again: visible for one frame, then
scale 0 again. Exactly "shows me for a frame until i am shot again".

The two bugs compound: at timeScale 0.1 the player leaves the trigger 10x slower in real time, so the
recapture window is wide open — which is why both symptoms show up together when entering mid-drag.

Fix: ignore an interactable for `_recaptureCooldown` (serialized, 0.3 s) after shooting it, and clear
that ignore in `OnTriggerExit2D` so a genuine re-entry still works. Time-based, so it cannot deadlock
if the object is already outside the trigger when its collider is re-enabled (no enter/exit fires).

### Notes
- `PlayerStatus.ResetSwapState()` is dead code; `SetPlayerState(PlayerState.Idle)` in
  `InputHandler_InputCancelled` is the author's only intended cancel path, so RC1's fix now activates
  it. Watch whether cancelling a mid-air aim should restore Ball instead of Idle.
- `Cannon.OnDrawGizmosSelected` has an inverted guard (`if (_rotatingParts is { Count: > 0 }) return;`
  then dereferences `_rotatingParts[0]`) — editor-only, unrelated, left alone.
- Project has no test assemblies (`*.asmdef` with nunit/TestAssemblies), so no automated regression
  test was possible for this.

## Portal teleport: player drifts while shrunk (2026-08-26)

`LevelExitPortal.OnTriggerEnter2D` calls `PlayerBallMovement.FreezePhysics()` and then runs a DOTween
sequence that scales the player to `swirlScale` (0.1), spins it and `DOMove`s it into the portal.

`FreezePhysics()` only did: zero velocities + `bodyType = Kinematic`. **That is not a freeze.**
- A kinematic Rigidbody2D still receives collision callbacks.
- `BouncingObject.OnCollisionEnter2D` assigns `_rigidbody2D.linearVelocity = newDirection * speed *
  bounciness` with no state guard.
- A kinematic body ignores forces and collision response but **does honour a velocity you assign to
  it**, so it starts translating.

So hitting a wall right as the swirl starts gives the shrunk player a velocity that the physics step
integrates while the `DOMove` tween is also writing `transform.position`. The two fight, and the trail
renderer makes the drift obvious. `UnfreezePhysics()` zeroes the velocity at the end, which is why it
"restores properly" once the teleport completes.

Trigger/collision callbacks are dispatched in the same physics step with no guaranteed ordering
between them, which is why it only happens *sometimes* — the wall bounce has to land after
`OnTriggerEnter2D` ran.

Fix: `FreezePhysics()` now also sets `_rigidbody2D.simulated = false` (restored from
`_preFreezeSimulated` in `UnfreezePhysics`). Removing the body from the simulation kills every
velocity-writing path for the duration of the swirl, not just the bounce one. `FreezePhysics` /
`UnfreezePhysics` are only called by the two portals, so the blast radius is the teleport itself.

Follow-up (user decision: arrive at rest): `BouncingObject._currentVelocity` used to keep the
pre-teleport speed across a teleport — `SetCurrentVelocity` refuses near-zero values and `FixedUpdate`
early-outs while the body is at rest, so nothing ever cleared it. `UnfreezePhysics()` zeroed the
rigidbody but not the bounce tracker, so a bounce on arrival would reuse the old speed. Added
`BouncingObject.ResetVelocityTracking()` (an explicit clear, since `SetCurrentVelocity` cannot express
zero) and called it from `UnfreezePhysics()`.

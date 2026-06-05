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

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

## Code Style Reminders
- Always encapsulate fields: private `_camelCase` backing field + public `PascalCase` property
- Never expose raw fields as public when a property will do
- Example: `private List<T> _allItems;` + `public List<T> AllItems => _allItems;`

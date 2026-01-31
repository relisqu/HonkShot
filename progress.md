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

## 5-Question Reboot Check
| Question | Answer |
|----------|--------|
| Where am I? | Session complete |
| Where am I going? | User verification in Unity editor for all new items/changes |
| What's the goal? | Item system improvements, new items, HealthController event cleanup |
| What have I learned? | See findings.md — item system, health events, fire system, pool patterns |
| What have I done? | See session log above — all tasks complete |

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

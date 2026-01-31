# Task Plan: Item System Improvements

## Goal
Improve the item system's debug tooling and pool management — safer debug spawning + a reusable ItemPoolSO ScriptableObject.

## Current Phase
Complete (both tasks delivered)

## Phases

### Phase 1: DebugMode — Safe item spawning
- [x] Read `DebugMode.cs` to understand current `SpawnItemWithIndex()`
- [x] Replace `.First()` with `.FirstOrDefault()` + null check
- [x] Add `Debug.LogError` on not-found, `Debug.Log` on success
- **Status:** complete

### Phase 2: ItemPoolSO — Pool ScriptableObject
- [x] Explore `ItemManager`, `PlayerItemSO`, `PlayerInventory` architecture
- [x] Design `ItemPoolSO` with editor button to auto-load items
- [x] Create `ItemPoolSO.cs`
- [x] Modify `ItemManager.cs` to use pool SO, remove old `LoadItems()`
- **Status:** complete

### Phase 3: Verification (user-side)
- [ ] Create ItemPool asset in Unity (Create > ScriptableObjects > ItemPool)
- [ ] Click "Load All Items" button on the asset
- [ ] Assign asset to `ItemManager.itemPool` in scene
- [ ] Enter Play mode and verify items load correctly
- **Status:** pending (requires Unity editor)

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| `FirstOrDefault` + null check instead of try/catch | Cleaner, avoids exception overhead, matches project style |
| `ItemPoolSO` as separate SO (not embedded in ItemManager) | Reusable across scenes, designer-friendly, matches existing `StagePoolSO` pattern |
| Keep `allItems` as runtime list on ItemManager | Filtered (enabled-only) at Awake — no downstream code changes needed |
| Remove `LoadItems()` from ItemManager | Responsibility moved to SO's editor button, avoids duplication |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| (none) | — | — |

## Notes
- Existing pool pattern precedent: `StagePoolSO` in LevelGeneration
- Items live in `Resources/Data/Items/` (5 assets currently)
- `PlayerItemSO.Enabled` flag controls whether item enters runtime pool

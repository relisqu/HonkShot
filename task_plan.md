# Task Plan — Room prefab regeneration button (FloorConfigSO)

## Goal
`FloorConfigSO` needs a working "Refresh Rooms" button that rebuilds the generated walls and
corner decorations inside every room **prefab** of that floor, so a change to `Wall.prefab`
does not require opening each room and clicking "update level" by hand.

Reported symptoms: the existing button "creates a lot of garbage in the scene and still does
not spawn objects".

## Root Cause (confirmed)
`RefreshRooms()` called the generators **directly on the prefab asset**
(`roomModel.RoomPrefab.GetComponent<...>()`).
- `CornerGenerator.Generate()` does `new GameObject(...)` + `SetParent(spawnParent)`.
- Unity forbids parenting a transform into a Prefab Asset, so every spawned object was created
  and then left **loose in the currently open scene** → the "garbage", and nothing landed in the prefab.
- Nothing called `SetDirty` / `SaveAsPrefabAsset` either, so even the spline edits from
  `CopySpriteShapeSpline()` were not persisted.

Secondary cause of the original pain: `WallGeneration.GenerateWalls()` used plain `Instantiate`,
so walls were disconnected copies — Wall prefab edits could never propagate.

## Phases

### Phase 0: Commit pending work to dev
- [x] Staged everything, committed as `9fa0347` "Extract aim camera logic and tune throw trajectory"
- [x] NOT pushed (user manages pushes)
- **Status:** complete

### Phase 1: Route the button through the Editor assembly
- [x] `FloorConfigSO.RefreshRoomsRequested` static Action (mirrors the existing `PreviewRequested`
      pattern used by `FloorPreviewer`) — runtime assembly cannot reference `UnityEditor`
- [x] `FloorConfigSO.AllRoomPrefabs()` — distinct rooms from RoomModels + micropools + boss rooms
- [x] Button guards Play Mode and a missing subscriber
- **Status:** complete

### Phase 2: Editor-side regeneration
- [x] New `Assets/_Project/Scripts/Editor/RoomPrefabRefresher.cs` (`[InitializeOnLoad]`)
- [x] Per room: `PrefabUtility.LoadPrefabContents` → run `CopySpriteShapeSpline` + `GenerateWalls`
      on every `WallGeneration`, `Generate()` on every `CornerGenerator` (GetComponentsInChildren,
      includeInactive) → `SaveAsPrefabAsset` → `UnloadPrefabContents` in `finally`
- [x] Cancelable progress bar, per-room try/catch so one bad room does not abort the run,
      summary log with refreshed/skipped counts
- **Status:** complete

### Phase 3: Make walls real prefab instances
- [x] `WallGeneration.SpawnWall()` — `PrefabUtility.InstantiatePrefab` in edit mode,
      plain `Instantiate` at runtime (and as fallback if the reference is not a prefab asset)
- [x] Moved the null-reference guard above the first `_levelField.spline` use (it was dead code
      after an NRE); added the same guard to `CopySpriteShapeSpline`
- **Status:** complete

### Phase 4: Verification
- [x] Compiled `Assembly-CSharp` and `Assembly-CSharp-Editor` with Unity's bundled Roslyn using
      the project's own Bee .rsp files → 0 errors
- [x] User ran it on FloorConfig_01: no scene garbage, objects placed correctly
- [x] Verified in YAML that all 18 FloorConfig_01 rooms now hold nested Wall.prefab instances
      (0 plain wall GameObjects left)
- **Status:** complete

### Phase 5: Reach rooms no config references
- [x] Found that 24 of 30 `Rooms/SecondFloor/*` prefabs are in no FloorConfig, so the config button
      cannot reach them
- [x] Added `Assets/Rooms/Refresh Selected Room Prefabs` menu item (selection + folder expansion,
      confirm dialog, skips prefabs without generators)
- [x] Compiles clean; Unity console clean after forced refresh
- [ ] User: select `Rooms/SecondFloor` (and FloorConfig_02's rooms) and run the menu item
- **Status:** awaiting user run

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| Static Action + `[InitializeOnLoad]` listener | No asmdefs in `_Project`, so the runtime assembly cannot call `UnityEditor`; matches the existing `PreviewRequested`/`FloorPreviewer` pattern |
| `LoadPrefabContents` instead of editing the asset in place | Only supported way to add/remove GameObjects inside a prefab asset from script |
| Walls spawned via `PrefabUtility.InstantiatePrefab` | Keeps the prefab link, so future Wall prefab edits propagate with no regeneration at all |
| Refresh boss + micropool rooms too, deduped | "Every room of this floor" is the user's intent |
| Dropped `Start/StopAssetEditing` batching | Batched imports around `LoadPrefabContents`/`SaveAsPrefabAsset` risks stale reads for little gain |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| `dotnet build` — no .NET SDK installed | 1 | Used Unity's bundled `NetCoreRuntime/dotnet.exe` + `DotNetSdkRoslyn/csc.dll` with the project's Bee `.rsp` files |
| Editor assembly would link against a stale `Assembly-CSharp.ref.dll` | 1 | Rewrote the `-r:` line in the copied .rsp to point at the freshly built ref assembly |

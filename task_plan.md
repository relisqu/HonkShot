# Task Plan — Run seed (generation + items + rerolls), shown in Settings, debug print

## Goal
One run seed drives floor layout, item-screen chance, item offers, rerolls and extra picks so a run
can be replayed. Seed is visible in the Settings panel (copyable) and a seed can be entered to replay.
DebugMode can print the seed.

## Audit (2026-11-19) — what was seeded before
| System | Before | After |
|---|---|---|
| Floor layout (`LevelGenerator`) | `System.Random(RunSeed ^ floor)`, RunSeed = TickCount, never shown/settable, `StartRun` never called | RunSeed from `RunSeedService`, derived per floor |
| Item screen chance (`LevelTransitionManager.ShouldGiveItems`) | `UnityEngine.Random` | seed stream (completedRooms) |
| Item offers (`ItemManager.GetRandomItems`) | `UnityEngine.Random` | seed stream (screenIndex, refreshIndex) |
| Rerolls / extra-pick refresh | reuse unseeded offers | refreshIndex++ on same screen stream |
| Extra pick chance (`ExtraPickSystem`) | `UnityEngine.Random` | seed stream (screenIndex, pickIndex) |
| Combat RNG, decorations, audio | unseeded | left unseeded on purpose (not generation) |

## Decisions
- Seed text = 8 uppercase hex chars; any other text typed is hashed (word seeds).
- Each system has its own derived stream so a reroll never shifts later floors/items.
- Pending seed is static (ProjectMonoInstaller is a scene installer, rebuilt on reload).
- Restart button = new random seed (unchanged). Settings "Play" = restart with typed seed.
- Seed row in Settings: label | input (prefilled with current seed, editable) | Copy | Play.
- Debug: F8 (DebugMode.DebugEnabled) / Odin button PrintSeed -> logs + copies to clipboard. Run start always logs `[RunSeed] Run started with seed X`.
- Offers still depend on player choices (owned items are filtered) — same choices => same run.

## Phases
| # | Phase | Status |
|---|-------|--------|
| 1 | SeedUtility, RunSeedService, SceneRestarter (MCP create_script) | complete |
| 2 | Wire: installer, RunProgress, LevelManager, LevelTransitionManager, ItemManager, ItemSelectionUI, ExtraPickSystem | complete |
| 3 | DebugMode: F8 + Odin button print seed | complete |
| 4 | SeedSettingsView script + localization keys | complete |
| 5 | (MCP) Seed row in PauseMenu prefab SettingsPanel | complete |
| 6 | Console compile check | complete (0 errors) — awaiting user playtest |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|

---
Previous task (Pause menu + Settings, 2026-11-09): complete, see progress.md.

# LevelLab: 30 floor-1 rooms + automated playtest bot

## Intent
Create ~30 new, enjoyable floor-1 combat rooms from existing pieces, each built around one clear idea, in a
separate folder the user can playtest. Rooms must keep the ball flowing (no frequent speed drops, no stuck
ball) and take about as long to clear as existing floor-1 rooms.

## Goals / non-goals / anti-goals
- Goals: 30 room prefabs (variants of `Room.prefab`) in `Resources/Prefabs/Rooms/LevelLab_Floor1/`, a
  `FloorConfig_LevelLab` that holds them, an automated validator + Play Mode bot, a report.
- Non-goals: floor 2 (later), new enemies/objects, changes to game code, wiring into the real RunConfig.
- Anti-goals: rooms where the ball rattles in pockets and dies; enemies unreachable or hittable/hitting on
  entry; rooms that drag on far longer (or are trivially shorter) than existing rooms; debug code leaking
  into game scripts or shipped builds.

## Constraints found in code
- Bounciness per surface (speed kept per bounce): outer `Wall` 1.0, bouncers 1.1, shooter/walker enemies
  0.8, `InnerWallObject` 0.9, `SmallField`/`SmallWall` 0.5. => no SmallField/SmallWall; islands only via
  `InnerWallObject` sparingly; prefer outline shape + bouncers.
- Throws: `InputHandler.OnDragStarted` -> Swapping (aim, needs a dash charge); `OnDragFinished(drag)` ->
  `ThrowGoose`, velocity = -drag.normalized * (current speed + min(|drag|, max)*mod), capped. Players
  re-throw mid-flight to redirect/boost; the bot must do this too.
- Trajectory preview only sees objects under `LevelObjects`; enemies must live there.

## Approach (chosen)
Data-driven room recipes -> editor builder -> static validator -> Play Mode bot, iterate.
Rejected: hand-placing 30 rooms via individual MCP calls (slow, error-prone); parallel agents editing the
editor at once (one Unity instance, conflicting state).

## Design
1. **Folder** `Assets/_Project/LevelLab/` (`Editor/`, `Runtime/`, `Recipes/`), namespace
   `Scripts.LevelLab`, all code under `#if UNITY_EDITOR`. No game scripts or tests are modified.
2. **Recipes**: JSON per room: id, name, idea, difficulty (1-3), outline points, spawn, exit, objects
   (prefab key, pos, rot, scale). Builder creates a `Room.prefab` variant, sets the `Field` spline, spawn and
   exit, instantiates prefabs under `LevelObjects`, regenerates walls/corners like `RoomPrefabRefresher`.
3. **Validator**: enemies/objects inside outline; enemies >= ~4u from spawn and not on the straight entry
   line; no overlaps with walls/each other; obstacle spacing >= ~3 ball widths; outline angles >= ~60°; no
   corridor narrower than ~2.5u; no 0.5-bounciness surfaces; room area within the range of existing rooms.
4. **Bot** (Play Mode, debug only): drives the real input path. Policy: from rest or mid-flight, when a dash
   charge exists and speed is low / a good line exists, evaluate candidate directions (and strengths) with
   a cheap 2D circle-cast bounce simulation and pick the one hitting the most enemies soonest; re-throw
   mid-flight. Metrics per room: clear time, throws, mid-flight re-throws, avg speed, sharp speed drops
   (>40% loss in one contact), stuck events (Ball state, speed < threshold, no dash for > N s, or >K bounces
   in 1 s in a small area), damage taken, timeout. Player made invulnerable for measurement (damage still
   counted).
5. **Baseline**: bot plays the 17 existing FloorConfig_01 rooms first. Target per new room: clear time
   within ~±30% of baseline median (difficulty 3 may go to +50%), zero stuck events, sharp-drop rate not
   above baseline.
6. **Design**: 30 concepts (10 D1 / 12 D2 / 8 D3) drawn from research archetypes (pillar, colonnade,
   bumper hall, split arena, pocket bank shot, chicane, funnel, cross, arrows lanes, cannon, holes…), one
   idea per room; drafted by 3 subagents (10 each) against the rules above, then built/validated/bot-tested
   and adjusted by me.

## Left to the builder
Exact recipe numbers, bot sampling resolution, thresholds tuning against baseline.

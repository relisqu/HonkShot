# LevelLab room recipe brief v2 (floor 1)

Read `_BRIEF.md` first: output format, coordinates, size, catalog and hard rules still apply.
This file adds the v2 rules from the user's review. Where they conflict, v2 wins.

## What the user asked for
1. Rooms with no danger are allowed sometimes, but they must be tagged so a floor never fills up with them.
2. Many more **sliders** (`MoveRoad`) and **cannons** (`Cannon`).
3. No corner traps: a goose sliding along the border, or hitting a tilted bouncer, must never rattle in a
   wedge between a bouncer and a wall (or between two bouncers).
4. Power pickups (`PointObject`, `Arrow`) must not be forgotten, and every slider/cannon must have a purpose:
   it launches the goose into points, into enemies, or onto a dangerous line.

## New / changed recipe fields
- `"chill": true` marks a breather room. Chill rooms contain **no danger**: no Shooter, SpreadShooter,
  LaserShooter, Follower, Walker, WalkerSpot or Hole (flies only). Every non-chill room has at least 2 danger
  enemies. At most 1 chill room per batch of 8. Chill rooms should still be fun: sliders, cannons, points.
- `MoveRoad` objects take a `path`: flat list of x,y points (room coordinates, 2–5 points), in **travel
  direction**. Omit `x`/`y` for a MoveRoad. Example:
  `{"key":"MoveRoad","path":[7.3,4.8, 12.0,4.4, 12.8,1.2, 11.6,0.0, 7.0,-0.5]}`
  The goose is pulled onto the path when within ~0.8u of it, carried along it with extra speed, and leaves
  along the direction of the last segment. Keep paths ≥ 1.2u from walls and solids, total length 4–14u,
  turns gentle (no corner sharper than ~100° between segments).
- `Cannon`: `rot` aims it. rot 0 fires toward +y, rot 90 toward −x, rot −90 toward +x, 180 toward −y.
  It sweeps ±30° around that aim and fires automatically when the goose enters it (and gives power).

## Purpose rule (validator checks it)
- The **exit** of every slider (last point, along its last segment) must lead, within 2 bounces, into an
  enemy or through a PointObject/Arrow.
- A cannon's aim (checked at −30°, 0°, +30°) must lead into an enemy or a point in at least 2 of 3 directions.
- Every room (except tiny boss-like ones) has 2–5 `PointObject`s or an `Arrow` line of 3–5, placed on a route
  that rewards a good move: at the exit of a slider, along a bank shot, across a cannon's sweep, or as bait
  near danger. Never in a dead corner.

## Corner-trap rules (validator checks them by simulating border runs)
- A solid object (any bouncer, InnerWall) is either **flush** against a wall (touching it, an edge parallel to
  the wall) or **≥ 2.2u** away from every wall and every other solid. Nothing in between.
- **Tilted bars (`BouncerV` with rot not 0/90/180) must be ≥ 2.2u from every wall** (no flush exception: a
  tilted bar against a wall makes a V-wedge).
- No bouncer within 3u of a room corner whose interior angle is < 110°.
- Bar bouncer orientation reminder: `BouncerV` is horizontal at rot 0 (use it for all bars). Do not use
  `Bouncer`/`Bouncer1` in v2 recipes (their native angles differ).

## Mix targets for a batch of 8
- ≥ 5 rooms with sliders, ≥ 3 rooms with cannons, ≥ 1 room with both.
- Turrets (Shooter/Spread/Laser) in ≥ 4 rooms, including D1 rooms (1 turret, well protected from spawn).
- Difficulty spread and enemy counts as in `_BRIEF.md`. Bot clear time target: 8–16 s (existing median 12 s).
  Fly-only/very open rooms clear in 2–5 s, which is too fast; rooms with 3+ turrets in pockets drag past 25 s.

## Patterns from the user's own rooms
- **Rectangle**: two U-shaped sliders hug the right wall and return toward the middle where two WalkerSpots
  and two Followers stand; PointObjects sit by the opposite wall next to a BigFly.
  Sliders: `[7.3,4.8, 12.0,4.4, 12.8,1.2, 11.6,0.0, 7.0,-0.5]`, `[7.1,-6.3, 12.2,-5.9, 12.4,-1.5, 6.9,-1.2]`.
- **n-shaped**: sliders run along the top wall and bend down into the two legs where enemies wait:
  `[-4.0,4.7, -10.5,4.6, -10.6,0.7]` and mirrored.
- **Room 2**: a short slider in the middle `[0.4,-2.1, 2.5,0.1, 6.7,-0.8]` fires at a Follower; four
  PointObjects line a diagonal row of bouncers as a bank-shot reward; an Arrow trio points at a corner.
- **DoubleArrows**: two cannons near the bottom corners (rot 0, firing up) launch the goose at a fly and a
  WalkerSpot higher up.

# LevelLab room recipe brief (floor 1)

You are designing combat rooms for HonkShot, a top-down 2D roguelike. The player is a goose that is
launched like a slingshot ball (drag-aim, trajectory preview) and bounces off walls; it can re-throw
mid-flight (each throw costs a dash charge; 3 charges, regenerating) to change direction and add speed.
Hitting enemies with the ball damages them. A room is cleared when every enemy is dead.

## Output format
One JSON file per room at `Assets/_Project/LevelLab/Recipes/LL_<NN>_<PascalName>.json`:

```json
{"id":"LL_07","name":"BankAlcove","idea":"one-line design intent","difficulty":1,
 "outline":[x0,y0, x1,y1, ...],
 "spawn":[x,y], "exit":[x,y],
 "objects":[{"key":"Walker","x":3.0,"y":2.5}, {"key":"Bouncer1","x":-4,"y":0,"rot":45}]}
```
- All coordinates are room-local world units, y up. The room's centre should be near (0,0).
- `outline`: closed polygon of the room's outer wall, flat list of x,y pairs, no repeated last point.
  Either winding is fine. Straight edges only (each pair of points becomes one wall).
- `rot`: degrees around Z (optional). `sx`,`sy`: optional scale (default 1). Keep enemies at scale 1.
- `exit`: where the exit portal appears after clearing (inside the room, ≥1.5u from walls).
- `door` optional; omit it.

## Size and scale
- Ball radius 0.35 (diameter 0.7). Existing floor-1 rooms: area 109–343 u², typical bounding box
  ~20 × 14 u (e.g. x -10..10, y -7..7). Keep area within 130–330.
- Existing rooms for reference (outline + objects in the same coordinate system):
  `Assets/_Project/LevelLab/Reports/floor1_examples.txt` — read it, match their scale and density.

## Object catalog (keys)
Enemies (floor 1):
| key | behaviour | collider |
|---|---|---|
| SmallFly | weak flyer, drifts towards the player; ball passes through it | trigger r0.6 |
| BigFly | tougher flyer, same behaviour | trigger r0.6 |
| Walker | ground walker, contact damage; ball bounces off it (0.8 speed kept) | solid r0.37 |
| WalkerSpot | walker that patrols between 3 spots around its position (~2–3u) | solid r0.37 |
| Follower | chaser ("maniac"), runs at the player; ball bounces off it (1.0) | solid r0.34 |
| Shooter | stationary turret, aimed single shots | trigger r0.33 |
| SpreadShooter | stationary turret, rotating spread of bullets | trigger r0.33 |
| LaserShooter | stationary turret, laser beam with wind-up | trigger r0.29 |

Level objects:
| key | what | footprint |
|---|---|---|
| Bouncer, Bouncer1, BouncerV | bar bouncer, speeds ball up ×1.1 | box 1.93 × 0.50 (rot rotates it) |
| Bouncer2 | block bouncer ×1.1 | box 1.35 × 1.25 |
| Bouncer3 | small triangle-ish bouncer ×1.1 | ~1.0 × 1.0 |
| Bouncer4 | small bouncer ×1.1 | ~0.96 × 1.29 |
| BouncerV1 | larger polygon bouncer ×1.1 | ~1.53 × 1.55 |
| SphereBouncer | round bumper ×1.1 | capsule 1.47 × 1.23 |
| InnerWall | solid island (pillar), keeps 0.9 speed | ~5.5 × 4.4 polygon. MAX ONE per room |
| Arrow | touch target (no physics), lights up when touched, feeds the fire meter. Use in lines of 3–6 to suggest a route | 0.8 × 0.7 |
| PointObject | touch target, feeds the fire meter | trigger r0.38 |
| Cannon | captures the ball, swings ±30°, fires it out | trigger r0.36 |
| Hole | hazard: player falls in, takes damage, is sent back to spawn | trigger 0.8 × 0.6 |

FORBIDDEN: SmallField, SmallWall, MoveRoad, anything not in the tables.

## Hard rules (a validator will reject violations)
1. Every object strictly inside the outline; solid objects must not overlap walls or each other.
2. Every enemy ≥ 4.5u from spawn. No ranged enemy (Shooter/Spread/Laser) with a clear straight line to
   the spawn within 7u. The first kill should need a bank shot or a short move.
3. Outline: no convex corner sharper than 60° (acute wedges trap the ball); no two non-adjacent edges
   closer than 3.0u anywhere (no narrow corridors); prefer corridors/arms ≥ 4u wide.
4. Gaps between a solid object and a wall/another solid: either < 0.7 (sealed; ball can't enter) or
   ≥ 2.2u. Nothing in between (tight slots make the ball rattle and lose speed).
5. Spawn ≥ 1.5u from any wall; exit inside the room.

## Feel rules (the user's priority)
- The ball must keep flowing: avoid pockets/alcoves narrower than ~3.5u, avoid clusters of solid
  enemies packed in a corner (each enemy hit keeps only 80% speed), never use speed-killing walls.
  Bouncers (×1.1) are the tool for keeping energy up; outer walls keep 100%.
- Rooms should take about as long as existing floor-1 rooms: 3–6 enemies for difficulty 1, 5–9 for
  difficulty 2, 7–12 for difficulty 3 (flies count as lighter than turrets/walkers).
- One clear idea per room, readable at a glance. Geometry should create interesting bank shots and
  reward mid-flight redirects, not mazes.
- Don't put turrets where the only way to hit them is a 3+ bounce perfect shot.
- Symmetry is fine but vary room silhouettes across your set; avoid plain rectangles for more than one
  room in your batch.

## Deliverable
Write your 10 JSON files, then reply with a table: id, name, difficulty, idea (one line), enemy list,
and anything you were unsure about. Do not touch any other files. Do not run Unity.

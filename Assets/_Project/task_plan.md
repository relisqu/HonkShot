# Task Plan: Inner Wall Generation System

## Goal
Create an inner wall/hole generator similar to WallGeneration.cs but for inner obstacles:
- GameObject w/ child SpriteShape that renders inner "hole" using current floor color
- Helper SpriteShape offset with top points 0.2 higher (visual depth)
- Generates walls with thin colliders around the shape so the ball bounces off properly
- Spawns random trees/details inside bounds, no overlap with outer field
- Updates in editor when source SpriteShape is edited
- Logically a solid obstacle (walls+details), visually a hole

## Phases
- [x] Phase 1: Explore WallGeneration.cs + related level systems (floor color, ItemManager-like patterns)
- [x] Phase 2: Design InnerWallGeneration script
- [x] Phase 3: Write C# script
- [x] Phase 4: Create prefab via MCP
- [x] Phase 5: Verify/link

## Key Decisions (to refine)
- Reuse the wall prefab used by WallGeneration for consistency (thin collider bouncing)
- Floor color source: FloorConfigSO / CurrentFloor?
- Use Random.insideUnitCircle + point-in-polygon check against helper shape for tree placement

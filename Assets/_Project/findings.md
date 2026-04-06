# Findings

## WallGeneration.cs Pattern (outer field)
- `[ExecuteInEditMode]` + Update() throttled by Time.time → calls GenerateWalls each ~1s in editor
- CopySpriteShapeSpline: clones src spline to dst, per-point height decision based on world-space averaged normal (top → 0.1, bottom → 0.5)
- GenerateWalls: sets corners thickness to 0.1, iterates spline segments, instantiates _wallPrefab at midpoint, rotates (90 + atan2), scales Y by edge length
- Wall prefab has BoxCollider2D, BounceObject, SpriteRenderer, Rigidbody2D(kinematic/massive) — guid d8ac461deff84a48b6db99929b71e868

## Floor Color Binding
- `LevelManager.Instance.FloorEntered` Action<FloorConfigSO>
- `FloorConfigSO.BackgroundColor` = camera clear color (used by FloorVisualController to set Camera.main.backgroundColor)
- Rooms are instantiated BEFORE FloorEntered fires inside EnterFloor() (GenerateFloor → FloorEntered), so subscribing in Start() works for the FIRST floor
- Pattern: subscribe in Start, unsubscribe in OnDestroy (per FloorVisualController)
- `SpriteShapeController.GetComponent<SpriteShapeRenderer>().color` for fill color

## Polygon-Inside Test
- BackgroundPropSpawner implements IsPointInsidePolygon (odd-even rule) — reusable pattern

## Code Style (CLAUDE.md)
- Use `if (component)` not `if (component == null)`
- Numeric constants should be SerializeField for designer tweaking
- Avoid excessive method comments
- Event subscribers named `ClassName_EventName`

## Inner Wall Requirements (this task)
- Parent GO holds script; child SpriteShape is the editable "hole"
- Helper SpriteShape: top spline points offset +0.2 world Y (fake depth rim)
- Walls along inner border with thin colliders for ball bounce
- Trees inside the hole polygon, must fit without crossing outer field
- Auto-refresh in editor when user edits the inner spline
- Color sampled from current floor (FloorConfigSO.BackgroundColor)

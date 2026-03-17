# Progress Log

## Session: 2026-03-17

### Research Phase - COMPLETE
- Read all enemy scripts, health system, shield system, invincibility system
- Identified what exists vs what needs to be created

### Implementation - COMPLETE

#### Shared Components Created:
- `Scripts/Enemies/Swamp/HidingBehavior.cs` - Timer-based hide/show, invincibility + collider disable + fade
- `Scripts/Enemies/Swamp/JumpingMovement.cs` - Frog-like jump with wall detection, customizable distance/duration/pause
- `Scripts/Enemies/Swamp/EnemyArmor.cs` - Hit-count armor, blocks hits above damage threshold, no regen

#### Enemy Scripts Created:
- `Scripts/Enemies/Swamp/SwampFlyEnemy.cs` - Path-following fly (YoYo/Closed), no attack
- `Scripts/Enemies/Swamp/SwampHidingShooter.cs` - Shooter + HidingBehavior integration (renamed from SwampShooterEnemy due to Unity type conflict)
- `Scripts/Enemies/Swamp/SwampWalkingEnemy.cs` - Jumps along waypoints + attack + armor
- `Scripts/Enemies/Swamp/SwampManiacEnemy.cs` - Jump-chase player + wall targeting + armor
- `Scripts/Enemies/Swamp/SwampArmoredHider.cs` - No attack, lots of armor, hides, bounces player

#### Modified:
- `Scripts/Health/InvincibilityEnum.cs` - Added `Hiding = 50` tag

#### Prefabs Created in `Resources/Prefabs/Enemies/Swamp/`:
- SwampFlyEnemy.prefab (from SmallFlyEnemy) - VARIANT, needs unpack
- SwampShooterEnemy_Default.prefab (from ShootingEnemyBase) - VARIANT, needs unpack
- SwampShooterEnemy_Spread.prefab (from ShootingEnemyBase) - VARIANT, needs unpack
- SwampWalkingEnemy.prefab (from WalkingEnemy Variant) - VARIANT, needs unpack
- SwampManiacEnemy.prefab (from Enemy) - Regular (not variant)
- SwampArmoredHider.prefab (from Enemy) - Regular (not variant)

### Known Issues
- 4 prefabs are still Variants of Enemy.prefab - need manual "Unpack Completely" in Unity editor
- Prefab serialized field references need wiring in Inspector (components were added but fields not linked)

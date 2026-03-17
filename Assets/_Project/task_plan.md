# Task Plan: Second Floor "Swamp" Enemies

## Goal
Create 5 new enemy types for the second floor (Swamp theme), with new scripts for hiding behavior, jumping movement, and armor integration.

## Phases

### Phase 1: New Shared Scripts `in_progress`
- [ ] `HidingBehavior.cs` - Reusable component: timer-based hide/show cycle, sets invincibility + optionally disables collider when hidden
- [ ] `JumpingMovement.cs` - Reusable component: frog-like jump toward target with customizable jump length, pause duration, jump speed. Wall-check before jump.
- [ ] `EnemyArmor.cs` - Reusable armor component: blocks X hits if damage > threshold, doesn't regen after destroyed. Hit-count based.

### Phase 2: Enemy Scripts `not_started`
1. **SwampFlyEnemy.cs** - Follows path (like WalkingEnemy) but flying. No standing still. No attack. Uses path points + WalkingType.
2. **SwampShooterEnemy.cs** - Extends ShooterEnemy with HidingBehavior. Two prefab variants (different configs).
3. **SwampWalkingEnemy.cs** - Jumps along waypoints (uses JumpingMovement + path system). Has armor.
4. **SwampManiacEnemy.cs** - Chases player with jumps (uses JumpingMovement). Wall-check for collision targeting. Has armor.
5. **SwampArmoredHider.cs** - Can't attack. Lots of armor. Hides periodically. Non-colliding when hidden. Player bounces off it.

### Phase 3: Prefab Setup via Unity MCP `not_started`
- Create `Resources/Prefabs/Enemies/Swamp/` folder
- Copy base prefabs as starting constructors (not variants)

## Key Design Decisions
- HidingBehavior uses HealthController.SetInvincible() with new InvincibilityEnum.Hiding tag
- JumpingMovement uses Rigidbody2D for physics, with raycast wall-check before jump
- EnemyArmor is standalone: tracks hit count, blocks hits above damage threshold, no regen
- All new enemies extend BaseEnemy for ID/score integration

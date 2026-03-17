# Findings

## Existing Systems
- ShieldController: blocks damage below threshold, destroys shield on hit above threshold. Used by boss only.
- HealthController.SetInvincible(tag, bool): uses BoolStatModifierSystem with InvincibilityEnum tags
- InvincibilityEnum: IFrames=-10, HonkMode=-100, Cannon=40. Need new tag for hiding.
- WalkingEnemy: path following with YoYo/Closed types, MoveTowards velocity
- ManiacEnemy: direct chase with raycast avoidance, Rigidbody2D velocity
- ShooterEnemy: uses ShootingModule + coroutine interval
- TowardsPlayerMover: simple chase component, not a BaseEnemy subclass
- SmallFlyEnemy prefab: NoAIEnemy + TowardsPlayerMover + Animator (flies toward player)
- BounceObject: collision-based bounce with animation, used by level objects

## Architecture Notes
- Enemies use PointReceiver injection with Instance fallback for player reference
- EnemyHealth handles damage reception from PlayerAttackController via triggers/collisions
- All enemies that are BaseEnemy subclass get score tracking
- Coroutine-based AI patterns are standard

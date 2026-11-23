# Hit Feedback (flash + squash wobble + attack-interrupt stun) — design

## Intent
Hitting an enemy or breakable object should feel punchy: white flash, juicy squash/stretch wobble,
and hitting a melee enemy mid-wind-up cancels its attack and stuns it briefly (rewarding aggression).

## Goals / non-goals / anti-goals
- Goals: flash + wobble on all damageable enemies and the breakable `DestructibleBouncer`;
  melee wind-up cancel + short stun.
- Non-goals: hit-stop, extra camera shake, shooter-enemy interrupts, boss changes.
- Anti-goals: colliders must NOT scale; must not break existing enemy behaviour, animators,
  hide/jump tweens; no white blobs from hidden sprites (attack-radius warning sprites).

## Approaches
- **A (picked):** new `HitFeedback` component (flash + wobble) replaces the old punch components on
  prefabs; old scripts stay untouched in the project. Stun lives in each melee enemy script.
- B: rewrite `EnemyDamagePunchEffect` / `SwampEnemyDamagePunchEffect` + separate flash script —
  more scripts, edits existing code, breakables still need a third.

## Design
### HitFeedback (`Scripts/VFX/HitFeedback.cs`)
- Listens to `HealthController.OnDamageReceived` (real damage only; shield-blocked hits don't flash).
- **Flash:** swaps the sprites under the visual to `BossHitFlash.mat` (FillColorShader, white) for
  `_flashDuration` (~0.08s), then restores each sprite's own material. Skips sprites that are disabled,
  inactive or fully transparent. Re-hit during a flash extends it.
- **Wobble:** DOTween punch on a scale *multiplier* (not on the transform), applied in `LateUpdate`
  on top of whatever the Animator / other tweens wrote that frame — so it composes with animated
  `Visual` scale, `JumpAnimator`, `EnemyTweenController` hide/show. Default punch (0.35, -0.3) for
  ~0.3s, vibrato/elasticity tweakable. A new hit restarts the wobble.
- Targets a visual child transform (`_visualTarget`), never the root -> colliders never scale.

### Stun (melee enemies: WalkingEnemy, SwampWalkingEnemy, ManiacEnemy, SwampManiacEnemy)
- Each subscribes to its own `HealthController.OnNonLethalDamageReceived`.
- Only if hit **during an attack wind-up** (`_isAttacking`): stop its coroutines, kill the warning
  sprite tween and hide it, stop warning particles, zero velocity, reset `Attack` trigger and return
  the animator to idle, wait `_hitStunDuration` (default 0.6s, per-enemy tweakable), then resume
  its normal behaviour with the attack timer restarted.
- Hits while not attacking: only flash + wobble (no stun), so jump/hide sequences are never cut.

### Prefab wiring (via UnityMCP)
- Replace `EnemyDamagePunchEffect` on 11 enemy prefabs (all except Boss) and
  `SwampEnemyDamagePunchEffect` on swamp hiders with `HitFeedback`; add `HitFeedback` to
  `DestructibleBouncer` targeting its `_visualTransform`.
- Boss keeps `BossVFXController` + its existing punch.

## Left to the builder
- Picking the right visual child per prefab (one with no colliders).
- Exact idle state names per animator controller (serialized, checked with `HasState`).

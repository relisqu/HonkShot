# Boss Animation System — Design

Date: 2026-06-05
Status: Approved — ready for implementation plan

## Goal

Add a frame-based Animator (Idle / Fly / Dash / PhaseChange) to the Ravage Boss without breaking the existing procedural DOTween effects (shot squash-stretch, dash trail). Add a 1-second phase-transition pause after the minions die. Replace the current black damage-flash with a material swap matching the bouncer's `PunchObjectAnimation`.

Sprite frame artwork is out of scope — clips are created empty and the user authors the frames.

## Boss phase recap (code terms)

- `_currentPhase == 0` — minions rotate around boss; boss is invincible, follows player, shoots single shots.
- `_currentPhase == 1` (entered via `GoToSecondPhase`) — minions dead, shields appear, boss vulnerable, still follows player and shoots.
- `_currentPhase == 2` (entered via `GoToThirdPhase`) — shields broken, boss moves between bezier waypoints, dashing periodically, shoots Single/Spread pattern.

The pause is added at the `0 → 1` transition (minions die). Not at `1 → 2` (shields broken).

## Architecture

### Visual hierarchy

Boss prefab structure changes:

```
EnemyTransform                      (root, Rigidbody2D, RavageBossEnemy)
└── Visual                          (BossVFXController scales this via DOTween)
    └── SpriteRoot   (NEW)          (Animator lives here; clips only animate SpriteRenderer.sprite)
        └── Circle                  (moved under SpriteRoot; has SpriteRenderer)
```

**Invariant:** Animator clips must NEVER animate any transform under `Visual`. They only animate `SpriteRenderer.sprite` frame swaps on `Circle`. This guarantees DOTween squash/stretch on `Visual.localScale` cannot conflict with frame animation.

### New Animator Controller — `BossAnimator.controller`

Location: `Assets/_Project/Resources/Prefabs/Enemies/StartFloor/Boss/Animations/BossAnimator.controller`

**States** (clips initially empty, user fills in sprite frames later):

| State | Clip | Looping | Notes |
|---|---|---|---|
| Idle | `Boss_Idle.anim` | Yes | Default state |
| Fly | `Boss_Fly.anim` | Yes | Moving (any phase) |
| Dash | `Boss_Dash.anim` | Yes | Only during phase-2 dash leg |
| PhaseChange | `Boss_PhaseChange.anim` | No | One-shot, ~1s length, exits to Idle |

**Parameters:**

| Name | Type | Driven by |
|---|---|---|
| `Speed` | float | `Rigidbody2D.linearVelocity.magnitude`, written every frame by `BossAnimationController` |
| `IsDashing` | bool | Set true during fast dash leg in `ThirdPhaseMovement`, false otherwise |
| `PhaseChange` | trigger | Fired by `BossAnimationController.PlayPhaseChange()` |

**Transitions** (no exit time unless stated, transition duration ~0.1s):

- `Idle → Fly` when `Speed > 0.1`
- `Fly → Idle` when `Speed < 0.1`
- `Any State → Dash` when `IsDashing == true`
- `Dash → Fly` when `IsDashing == false`
- `Any State → PhaseChange` when `PhaseChange` trigger fires
- `PhaseChange → Idle` exit time (clip completes)

The threshold `0.1` is the default value of `_idleThreshold` on `BossAnimationController` (serialized, tunable).

### New script — `BossAnimationController.cs`

Location: `Assets/_Project/Scripts/Enemies/Bosses/BossAnimationController.cs`
Namespace: `Scripts.Enemies.Bosses`

Responsibilities:
- Owns the Animator wiring.
- Writes `Speed` parameter every frame from rigidbody velocity.
- Exposes `SetDashing(bool)` and `PlayPhaseChange()` for `RavageBossEnemy` to call.

Serialized fields:
- `Animator _animator`
- `Rigidbody2D _rigidbody`
- `float _idleThreshold = 0.1f`

Animator parameter hashes (`Animator.StringToHash`) are cached in `Awake` to avoid string lookups in `Update`.

`Update()`:
- `var speed = _rigidbody.linearVelocity.magnitude;`
- `_animator.SetFloat(_speedHash, speed < _idleThreshold ? 0f : speed);` — snap-to-zero below threshold prevents idle/fly flicker.

Public API:
- `SetDashing(bool value)` → `_animator.SetBool(_isDashingHash, value);`
- `PlayPhaseChange()` → `_animator.SetTrigger(_phaseChangeHash);`

No event subscriptions; `RavageBossEnemy` is the orchestrator and calls in directly.

### Modifications — `RavageBossEnemy.cs`

**New serialized fields:**
```csharp
[SerializeField] private BossAnimationController _animationController;

[Header("Phase Transition")]
[SerializeField] private float _phaseTransitionDuration = 1f;
[SerializeField] private float _phaseTransitionShakeStrength = 0.3f;
[SerializeField] private int _phaseTransitionShakeVibrato = 20;
```

**New coroutine:**
```csharp
private IEnumerator PhaseTransitionRoutine(Action onComplete)
{
    // Pause boss
    _rigidbody2D.linearVelocity = Vector2.zero;
    _bossHealth.SetInvincible(0, true);

    // Visual feedback
    if (_animationController) _animationController.PlayPhaseChange();
    transform.DOShakePosition(
        _phaseTransitionDuration,
        _phaseTransitionShakeStrength,
        _phaseTransitionShakeVibrato);

    yield return new WaitForSeconds(_phaseTransitionDuration);

    onComplete?.Invoke();
}
```

`onComplete` calls the existing `GoToSecondPhase` body, which already calls `SetInvincible(0, false)` and adds the shields, so the invincibility lift happens at the right time inside the original method (no need to flip it here).

**Modify `Minion_Died`:**

Currently:
```csharp
if (_firstPhaseHealth > 0) { ... }
else { GoToSecondPhase(); }
```

Changes to:
```csharp
if (_firstPhaseHealth > 0) { ... }
else { StartCoroutine(PhaseTransitionRoutine(GoToSecondPhase)); }
```

Note: the `FollowPlayerCoroutine` and `ShootingCoroutine` loops gate on `_currentPhase < 2`, so they keep running during the pause. We don't stop them — we just zero velocity each frame? No — the FollowPlayerCoroutine writes velocity every frame regardless. So we need to actually suspend those during the pause.

**Therefore also add:** a `bool _isPaused` field. Both `FollowPlayerCoroutine` and `ShootingCoroutine` check it at the top of their loop body and `continue` (after `yield return null` for follow / `yield return new WaitForSeconds(...)` for shooting). `PhaseTransitionRoutine` sets `_isPaused = true` at the start and `false` after the wait, before invoking `onComplete`.

**Wire dash animation:**

In `ThirdPhaseMovement`, at each existing call to `_vfxController.SetDashTrailActive(...)`, add the matching call:
```csharp
if (_vfxController) _vfxController.SetDashTrailActive(true);
if (_animationController) _animationController.SetDashing(true);
// ... dash to point ...
if (_vfxController) _vfxController.SetDashTrailActive(false);
if (_animationController) _animationController.SetDashing(false);
```

### Modifications — `BossVFXController.cs`

Match the pattern in `Assets/_Project/Scripts/LevelSystem/LevelObjects/PunchObjectAnimation.cs`:

**Field changes:**
- Remove `[SerializeField] private float _flashDuration = 0.1f;` → keep, still used.
- Remove `private Color _originalColor;` — no longer needed.
- Add `[SerializeField] private Material _hitFlashMaterial;` in the Damage Flash header block.

**`Start()`:** remove the `_originalColor = _spriteRenderer.color;` line.

**Rename `FlashBlack` → `FlashHit`** and rewrite:
```csharp
private IEnumerator FlashHit()
{
    if (!_spriteRenderer || !_hitFlashMaterial) yield break;

    var previous = _spriteRenderer.material;
    _spriteRenderer.material = _hitFlashMaterial;
    yield return new WaitForSeconds(_flashDuration);
    _spriteRenderer.material = previous;
    _flashCoroutine = null;
}
```

Update the call site in `HealthController_OnDamaged` to call `FlashHit`.

`_spriteRenderer` must point to the `SpriteRenderer` on `Circle` (which is now under `SpriteRoot`). Re-wire this serialized field on the prefab.

## Asset & prefab work (via Unity MCP)

1. Create folder `Assets/_Project/Resources/Prefabs/Enemies/StartFloor/Boss/Animations/`.
2. Create empty `.anim` clips: `Boss_Idle`, `Boss_Fly`, `Boss_Dash`, `Boss_PhaseChange`. Set looping on first three.
3. Create `BossAnimator.controller`. Add the three parameters. Add the four states. Wire the transitions above. Default state = `Idle`.
4. Edit `Boss.prefab`:
   - Add new empty child `SpriteRoot` under `Visual`.
   - Reparent existing `Circle` under `SpriteRoot`.
   - Add `Animator` component on `SpriteRoot`; set `Controller` to `BossAnimator`.
   - Add `BossAnimationController` component on `EnemyTransform` (root); wire `_animator` to the new Animator, `_rigidbody` to root Rigidbody2D.
   - On `RavageBossEnemy`, wire new `_animationController` field.
   - On `BossVFXController`, wire `_hitFlashMaterial` (use the same material the bouncer uses).
   - Verify `BossVFXController._spriteRenderer` still resolves correctly after reparenting.

## Out of scope

- Authoring frame keyframes inside the .anim clips — user does this.
- Pause on the shields-broken (`1 → 2`) transition — easy to add later by wrapping `GoToThirdPhase` the same way.
- Boss intro animation hookup via Animator — `BossIntro` is unchanged.
- Tuning shake strength / pause duration — exposed as serialized fields, designer-tweakable.

## Testing

- Play scene, enter boss room.
- Kill minions one by one. On last minion death: verify 1s pause with shake, boss takes no damage during pause, then shields appear and boss resumes following.
- Break shields. Boss should enter phase 2 immediately (no pause), start moving along bezier curve.
- Watch a dash: Animator should show `Dash` state during the fast leg, `Fly` between dashes.
- Hit boss with player attack: sprite material briefly swaps to hit-flash material then restores.
- Verify the shot squash-stretch tween still plays (it lives on `Visual` transform, untouched by Animator).

# Hit Feedback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** White flash + squash wobble on hit for enemies and `DestructibleBouncer`; melee enemies hit during wind-up get their attack cancelled and a short stun.

**Architecture:** New visual-only `HitFeedback` component listens to `HealthController.OnDamageReceived`, swaps sprite materials for the flash and applies a DOTween-punched scale multiplier in `LateUpdate` on a visual child (composes with Animator/tweens, never scales colliders). Stun lives inside each melee enemy script, triggered by its own `OnNonLethalDamageReceived` while `_isAttacking`.

**Tech Stack:** Unity 2D, C# (Assembly-CSharp), DOTween, UnityMCP for prefab edits.

**Spec:** `docs/superpowers/specs/2026-11-23-hit-feedback-design.md`

## Global Constraints
- Colliders must never scale: the wobble target is a child transform with no `Collider2D` in its subtree.
- No Play Mode / builds / playtests by Claude — verification = compile check via `read_console` + edit-mode prefab inspection via MCP `execute_code` (C# 6).
- New scripts created via MCP `create_script`; prefabs edited via MCP only (never hand-written YAML).
- Old `EnemyDamagePunchEffect` / `SwampEnemyDamagePunchEffect` scripts stay in the project unchanged; only removed from prefabs.
- Boss prefab untouched.
- Project style: `_camelCase` private fields, `if (component)` checks, `Owner_Event` handler naming, unsubscribe in `OnDestroy`, tweakable values as `[SerializeField]`.
- No test framework exists (all code in Assembly-CSharp, no test asmdef); tests are replaced by MCP edit-mode checks.
- No git commits unless the user asks (checkpoint already at `368aedf7`).

## Review Focus
- Enemy killed / destroyed mid-flash or mid-wobble -> tweens killed in `OnDestroy`, no null refs (DOTween `SetLink(gameObject)`).
- Breakable dies (visual SetActive(false)) mid-flash -> materials restored state doesn't matter, no exceptions on inactive renderers.
- Hit lands while hider is in hidden (x-scale 0.1) state -> multiplier applies on top, returns exactly to whatever the tween controller writes.
- Stun while warning sprite DOColor tween is running -> tween killed, sprite stays clear (no ghost circle fading back in).
- Stun then enemy dies during stun -> resume coroutine never restarts behaviour on a dead enemy.

---

### Task 1: `HitFeedback` component

**Files:**
- Create: `Assets/_Project/Scripts/VFX/HitFeedback.cs` (namespace `Scripts.VFX`)

**Interfaces:**
- Produces: `HitFeedback : MonoBehaviour` with serialized fields
  `HealthController _healthController` (auto: GetComponent, then GetComponentInParent),
  `Transform _visualTarget`, `SpriteRenderer[] _flashRenderers` (empty -> `GetComponentsInChildren<SpriteRenderer>(true)` of `_visualTarget`),
  `Material _flashMaterial`, `float _flashDuration = 0.08f`,
  `Vector3 _punch = (0.35, -0.3, 0)`, `float _punchDuration = 0.3f`, `int _punchVibrato = 6`, `float _punchElasticity = 0.8f`;
  public `void Play()` (flash + wobble, also used by the subscription).

- [ ] **Step 1:** Create the script via MCP `create_script`.
  - Subscribe `_healthController.OnDamageReceived += HealthController_DamageReceived` in `Start`, unsubscribe in `OnDestroy`.
  - Flash: on first flash, cache each renderer's `sharedMaterial`; swap to `_flashMaterial` only for renderers that are `enabled && gameObject.activeInHierarchy && color.a > 0`; restore after `_flashDuration` (unscaled? no — scaled time, coroutine). Re-hit restarts the timer without re-caching.
  - Wobble: `DOTween.Punch(() => _offset, v => _offset = v, _punch, _punchDuration, _punchVibrato, _punchElasticity).SetLink(gameObject)`; kill previous punch first.
  - LateUpdate composition algorithm:
    ```
    current = target.localScale
    base = (_hasWritten && current == _lastWritten) ? Divide(current, _lastMultiplier) : current
    multiplier = Vector3.one + _offset
    if (!punchActive && _offset ~ 0) { if (_hasWritten) target.localScale = base; _hasWritten = false; return; }
    _lastWritten = Scale(base, multiplier); target.localScale = _lastWritten
    _lastMultiplier = multiplier; _hasWritten = true
    ```
    (Divide guards zero components by leaving them unchanged.)
- [ ] **Step 2:** `read_console` (Error) -> expected: no compile errors.

### Task 2: Melee wind-up stun

**Files:**
- Modify: `Assets/_Project/Scripts/Enemies/WalkingEnemy.cs`
- Modify: `Assets/_Project/Scripts/Enemies/Swamp/SwampWalkingEnemy.cs`
- Modify: `Assets/_Project/Scripts/Enemies/ManiacEnemy.cs`
- Modify: `Assets/_Project/Scripts/Enemies/Swamp/SwampManiacEnemy.cs`

**Interfaces:**
- Each gains `[SerializeField] float _hitStunDuration = 0.6f;` and `[SerializeField] string _idleStateName` (defaults: `"walkingEnemy_idle"` for WalkingEnemy, `"FollowerEnemy_idle"` for Maniac/SwampManiac; SwampWalkingEnemy: read its controller to pick the default).
- Each subscribes `HealthController.OnNonLethalDamageReceived += HealthController_NonLethalDamageReceived` (unsub in `OnDestroy`).

- [ ] **Step 1:** In each enemy, on non-lethal damage while `_isAttacking`:
  `StopAllCoroutines()`; prefire sprite `DOKill()` + `color = Color.clear` (where it exists); `_attackWarningEffect/_attackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear)`; `linearVelocity = 0`; `_animator.ResetTrigger("Attack")`, `SetBool("IsWalking", false)`, `Play(_idleStateName)` if `HasState(0, hash)`; `_isAttacking = false`; set `_isStunned = true`; start `StunRoutine`.
- [ ] **Step 2:** `StunRoutine`: wait `_hitStunDuration`; `_isStunned = false`; if still alive and enabled, restart the enemy's normal coroutines exactly as `OnEnable` does (extract `StartBehaviour()` from `OnEnable`) and set `_nextAttackTime = Time.time + _attackInterval` where that field exists. ManiacEnemy: `FixedUpdate` returns early with zero velocity while `_isStunned`. Jumpers: also confirm `JumpingMovement` isn't mid-jump (wind-up state implies grounded).
- [ ] **Step 3:** `read_console` (Error) -> no compile errors.

### Task 3: Prefab wiring (UnityMCP)

**Files (prefabs under `Assets/_Project/Resources/Prefabs/`):**
- Replace `EnemyDamagePunchEffect` -> `HitFeedback`: `Enemies/StartFloor/{BigFlyEnemy Variant, Boss/BossMinion, DefaultShooterEnemy Variant, FollowerEnemy Variant, LaserShootingEnemy Variant, SmallFlyEnemy, SpreadShooterEnemy Variant, WalkingEnemy Variant}`, `Enemies/Swamp/{SwampFlyEnemy, SwampManiacEnemy, SwampWalkingEnemy2nd}`
- Replace `SwampEnemyDamagePunchEffect` -> `HitFeedback`: every prefab that has it (find by script guid).
- Add `HitFeedback`: `LevelObjects/SwampObjects/DestructibleBouncer` (`_visualTarget` = its `DestructibleHealth._visualTransform`).
- Check `Rooms/Room 1.prefab` (contains a WalkingEnemy) — if it's a nested instance it inherits; if unpacked, wire it too.

- [ ] **Step 1:** For each prefab pick `_visualTarget`: the child holding the main SpriteRenderer (usually `Visual`) whose subtree has no `Collider2D`; set `_flashMaterial` = `Resources/Prefabs/Enemies/StartFloor/Boss/BossHitFlash.mat`. Base prefabs first, then variants (avoid duplicate components on variants inheriting from a wired base).
- [ ] **Step 2:** Edit-mode verification via `execute_code`: load each prefab, assert exactly one `HitFeedback`, zero old punch components, `_visualTarget != root`, no `Collider2D` under `_visualTarget`, `_flashMaterial` set. Print a table; expected all OK.
- [ ] **Step 3:** `read_console` -> no errors/warnings from import.

### Task 4: Handoff
- [ ] **Step 1:** File beads issue (label `playtest`) listing what to feel-test: flash timing, wobble strength, stun duration per enemy, hider + jumper interplay.
- [ ] **Step 2:** Close `hs-2vg`; leave `hs-g43` (shooter stun) open as next work.

using Scripts.Camera;
using Scripts.Health;
using UnityEngine;

namespace Scripts.VFX
{
    public class PlayerHitJuice : MonoBehaviour
    {
        [SerializeField] private PlayerAttackController _attackController;

        [Header("Hit Strength")]
        [Tooltip("Damage at or below this plays the lightest reaction.")]
        [SerializeField] private float _lightHitDamage = 10f;
        [Tooltip("Damage at or above this plays the strongest reaction.")]
        [SerializeField] private float _heavyHitDamage = 35f;

        [Tooltip("Hits below this damage (e.g. resting contact ticks) get no hit stop or shake. Kills always do.")]
        [SerializeField] private float _minJuiceDamage = 7f;

        [Header("Hit Stop")]
        [Tooltip("Freeze length in real seconds for the lightest (x) and heaviest (y) hit. Small kills use this too.")]
        [SerializeField] private Vector2 _hitStopRange = new Vector2(0.02f, 0.045f);
        [Tooltip("Real seconds after a freeze before a regular hit can freeze again.")]
        [SerializeField] private float _hitStopCooldown = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _hitStopTimeScale = 0.02f;

        [Header("Big Kill Hit Stop")]
        [Tooltip("Killing a target with at least this much starting health counts as a big kill.")]
        [SerializeField] private float _bigKillHealth = 80f;
        [Tooltip("Freeze length in real seconds for a big kill.")]
        [SerializeField] private float _bigKillHitStop = 0.075f;
        [Tooltip("Real seconds after a big-kill freeze before another big kill can freeze longer.")]
        [SerializeField] private float _bigKillCooldown = 1f;

        [Header("Camera Shake")]
        [SerializeField] private float _shakeDuration = 0.12f;
        [Tooltip("Shake strength for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _shakeStrengthRange = new Vector2(0.8f, 2.2f);

        private bool _lastHitWasBigKill;
        private bool _lastHitKilled;
        private float _hitStopReadyTime;
        private float _bigKillReadyTime;

        private void Awake()
        {
            if (!_attackController)
                _attackController = GetComponentInChildren<PlayerAttackController>();
        }

        private void Start()
        {
            if (!_attackController) return;
            _attackController.OnHit += AttackController_Hit;
            _attackController.OnDamaged += AttackController_Damaged;
        }

        private void OnDestroy()
        {
            if (!_attackController) return;
            _attackController.OnHit -= AttackController_Hit;
            _attackController.OnDamaged -= AttackController_Damaged;
        }

        private void AttackController_Hit(GameObject target)
        {
            _lastHitWasBigKill = false;
            _lastHitKilled = false;
            if (!target) return;

            var health = target.GetComponent<HealthController>();
            if (!health)
                health = target.GetComponentInParent<HealthController>();
            _lastHitKilled = health && !health.IsAlive;
            _lastHitWasBigKill = _lastHitKilled && health.DefaultHealth >= _bigKillHealth;
        }

        private void AttackController_Damaged(float damage)
        {
            bool bigKill = _lastHitWasBigKill;
            bool killed = _lastHitKilled;
            _lastHitWasBigKill = false;
            _lastHitKilled = false;
            if (damage <= 0f) return;
            if (!killed && damage < _minJuiceDamage) return;

            float heaviness = _heavyHitDamage > _lightHitDamage
                ? Mathf.InverseLerp(_lightHitDamage, _heavyHitDamage, damage)
                : 0.5f;

            TryHitStop(bigKill, heaviness);

            if (CameraShakeHandler.Instance)
                CameraShakeHandler.Instance.ShakeCamera(_shakeDuration,
                    Mathf.Lerp(_shakeStrengthRange.x, _shakeStrengthRange.y, heaviness));
        }

        private void TryHitStop(bool bigKill, float heaviness)
        {
            if (!TimeManager.Instance) return;

            float now = Time.unscaledTime;
            float duration;
            if (bigKill && now >= _bigKillReadyTime)
            {
                duration = _bigKillHitStop;
                _bigKillReadyTime = now + duration + _bigKillCooldown;
            }
            else if (now >= _hitStopReadyTime)
            {
                duration = Mathf.Lerp(_hitStopRange.x, _hitStopRange.y, heaviness);
            }
            else
            {
                return;
            }

            _hitStopReadyTime = now + duration + _hitStopCooldown;
            TimeManager.Instance.HitStop(duration, _hitStopTimeScale, bigKill);
        }
    }
}

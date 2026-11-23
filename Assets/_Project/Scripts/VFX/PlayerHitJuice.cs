using Scripts.Camera;
using Scripts.Health;
using UnityEngine;

namespace Scripts.VFX
{
    // Global feedback for the player's direct hits: hit stop and a small camera shake.
    // Hit strength and the weak-hit threshold come from the same HitFeedbackConfig the enemies use.
    public class PlayerHitJuice : MonoBehaviour
    {
        [SerializeField] private PlayerAttackController _attackController;
        [SerializeField] private HitFeedbackConfig _hitConfig;

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

        private float _hitStopReadyTime;
        private float _bigKillReadyTime;

        private void Awake()
        {
            if (!_attackController)
                _attackController = GetComponentInChildren<PlayerAttackController>();
            if (!_hitConfig)
                Debug.LogWarning($"{name}: PlayerHitJuice has no HitFeedbackConfig assigned, hit stop and shake are off.", this);
        }

        private void Start()
        {
            if (_attackController)
                _attackController.OnDamageDealt += AttackController_DamageDealt;
        }

        private void OnDestroy()
        {
            if (_attackController)
                _attackController.OnDamageDealt -= AttackController_DamageDealt;
        }

        private void AttackController_DamageDealt(HealthController target, float damage)
        {
            if (!_hitConfig || !target || damage <= 0f) return;

            bool killed = !target.IsAlive;
            if (!killed && !_hitConfig.IsFullReaction(damage)) return;

            float strength = _hitConfig.GetHitStrength(damage);
            bool bigKill = killed && target.DefaultHealth >= _bigKillHealth;
            TryHitStop(bigKill, strength);

            if (CameraShakeHandler.Instance)
                CameraShakeHandler.Instance.ShakeCamera(_shakeDuration,
                    Mathf.Lerp(_shakeStrengthRange.x, _shakeStrengthRange.y, strength));
        }

        private void TryHitStop(bool bigKill, float strength)
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
                duration = Mathf.Lerp(_hitStopRange.x, _hitStopRange.y, strength);
            }
            else
            {
                return;
            }

            _hitStopReadyTime = now + duration + _hitStopCooldown;
            TimeManager.Instance.HitStop(duration, _hitStopTimeScale);
        }
    }
}

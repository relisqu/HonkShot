using UnityEngine;

namespace Scripts.Enemies.Bosses
{
    public class BossAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _idleThreshold = 0.1f;

        private int _speedHash;
        private int _isDashingHash;
        private int _phaseChangeHash;

        private void Awake()
        {
            _speedHash = Animator.StringToHash("Speed");
            _isDashingHash = Animator.StringToHash("IsDashing");
            _phaseChangeHash = Animator.StringToHash("PhaseChange");
        }

        private void Update()
        {
            if (!_animator || !_rigidbody) return;

            var speed = _rigidbody.linearVelocity.magnitude;
            _animator.SetFloat(_speedHash, speed < _idleThreshold ? 0f : speed);
        }

        public void SetDashing(bool value)
        {
            if (_animator)
                _animator.SetBool(_isDashingHash, value);
        }

        public void PlayPhaseChange()
        {
            if (_animator)
                _animator.SetTrigger(_phaseChangeHash);
        }
    }
}

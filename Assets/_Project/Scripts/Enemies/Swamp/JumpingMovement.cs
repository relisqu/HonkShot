using System;
using System.Collections;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class JumpingMovement : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float _jumpDistance = 3f;
        [SerializeField] private float _jumpDuration = 0.4f;
        [SerializeField] private float _pauseBetweenJumps = 1f;

        [Header("Wall Detection")]
        [SerializeField] private float _wallCheckDistance = 1f;
        [SerializeField] private LayerMask _obstacleMask;

        [Header("Components")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private JumpAnimator _jumpAnimator;

        private bool _isJumping;

        public bool IsJumping => _isJumping;
        public float PauseBetweenJumps => _pauseBetweenJumps;
        public event Action OnJumpStart;
        public event Action OnJumpLand;

        private void Awake()
        {
            if (!_rb)
                _rb = GetComponent<Rigidbody2D>();
            if (!_jumpAnimator)
                _jumpAnimator = GetComponent<JumpAnimator>();
        }

        private void OnDisable()
        {
            // Coroutines die when the GameObject is disabled, but the _isJumping flag would
            // otherwise survive — leaving the enemy permanently locked in mid-jump state.
            _isJumping = false;
        }

        public bool CanJumpTowards(Vector2 direction)
        {
            var hit = Physics2D.Raycast(transform.position, direction, _wallCheckDistance, _obstacleMask);
            return !hit.collider;
        }

        public Vector2 FindWallDirection(Vector2 preferredDirection)
        {
            float[] angles = { 0, 30, -30, 60, -60, 90, -90, 120, -120, 150, -150, 180 };
            foreach (float angle in angles)
            {
                Vector2 dir = Quaternion.Euler(0, 0, angle) * preferredDirection;
                var hit = Physics2D.Raycast(transform.position, dir, _wallCheckDistance, _obstacleMask);
                if (hit.collider)
                    return dir;
            }
            return Vector2.zero;
        }

        public void JumpTowards(Vector2 direction)
        {
            if (_isJumping) return;
            StartCoroutine(JumpCoroutine(direction.normalized));
        }

        public void JumpToPosition(Vector2 targetPosition)
        {
            Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, targetPosition);
            if (distance > _jumpDistance)
                distance = _jumpDistance;

            if (_isJumping) return;
            StartCoroutine(JumpCoroutine(direction, distance));
        }

        private IEnumerator JumpCoroutine(Vector2 direction, float? overrideDistance = null)
        {
            _isJumping = true;
            OnJumpStart?.Invoke();

            try
            {
                if (_jumpAnimator)
                {
                    _jumpAnimator.PlayAnticipation();
                    yield return new WaitForSeconds(_jumpAnimator.AnticipationDuration);
                    _jumpAnimator.PlayLaunchStretch();
                }

                float distance = overrideDistance ?? _jumpDistance;
                Vector2 startPos = _rb.position;
                Vector2 endPos = startPos + direction * distance;

                float elapsed = 0f;

                while (elapsed < _jumpDuration)
                {
                    elapsed += Time.fixedDeltaTime;
                    float t = Mathf.Clamp01(elapsed / _jumpDuration);

                    Vector2 flatPos = Vector2.Lerp(startPos, endPos, t);
                    _rb.MovePosition(flatPos);

                    if (_jumpAnimator)
                        _jumpAnimator.UpdateArc(t);

                    yield return new WaitForFixedUpdate();
                }

                _rb.MovePosition(endPos);
            }
            finally
            {
                // Guarantee the flag clears even if something inside the loop throws —
                // otherwise the enemy gets permanently locked in the jumping state.
                _isJumping = false;
                OnJumpLand?.Invoke();
            }
        }
    }
}

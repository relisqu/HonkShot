using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class BouncingObject : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private Vector2 _currentVelocity;
        public Vector2 CurrentVelocity => _currentVelocity;

        private Vector2 _lastContactPoint;
        private Vector2 _lastNormal;
        private Vector2 _lastIncoming;
        private Vector2 _lastReflected;
        private bool _hasDebugData;

        public void ClearDebugData()
        {
            _hasDebugData = false;
        }

        public void SetCurrentVelocity(Vector2 velocity)
        {
            if (_currentVelocity.magnitude > 0.01f && velocity.magnitude < 0.001f) return;
            _currentVelocity = velocity;
        }

        private void FixedUpdate()
        {
            if (_rigidbody2D.linearVelocity.magnitude < 0.001f) return;
            _currentVelocity = _rigidbody2D.linearVelocity;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent(out BounceObject bounceObject)) return;

            var averageNormal = Vector2.zero;
            foreach (var contact in other.contacts)
            {
                averageNormal += contact.normal;
            }
            averageNormal = (averageNormal / other.contactCount).normalized;

            float speed = _currentVelocity.magnitude;

            if (other.rigidbody)
            {
                float relativeApproach = Mathf.Abs(Vector2.Dot(other.relativeVelocity, averageNormal));
                speed = Mathf.Max(speed, relativeApproach);
            }

            Vector2 newDirection;
            if (_currentVelocity.magnitude < 0.1f)
            {
                newDirection = averageNormal;
            }
            else
            {
                newDirection = Vector2.Reflect(_currentVelocity.normalized, averageNormal);
            }

            _rigidbody2D.linearVelocity = newDirection * speed * bounceObject.Bounciness;

            Vector2 contactPoint = Vector2.zero;
            foreach (var contact in other.contacts)
                contactPoint += contact.point;
            contactPoint /= other.contactCount;

            bool isGhost = gameObject.layer == LayerMask.NameToLayer("SimulationGhost");

            if (!isGhost || !_hasDebugData)
            {
                _lastContactPoint = contactPoint;
                _lastNormal = averageNormal;
                _lastIncoming = _currentVelocity.normalized;
                _lastReflected = newDirection;
                _hasDebugData = true;
            }

            if (!isGhost && other.gameObject.layer == 9)
                Debug.Break();
        }

        private void OnDrawGizmos()
        {
            if (!_hasDebugData) return;

            bool isGhost = gameObject.layer == LayerMask.NameToLayer("SimulationGhost");
            Vector3 origin = _lastContactPoint;

            if (isGhost)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastNormal * 2f));
                Gizmos.color = new Color(0.5f, 1f, 0.5f);
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastIncoming * 2f));
                Gizmos.color = new Color(0f, 1f, 0.5f);
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastReflected * 2f));
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastNormal * 2f));
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastIncoming * 2f));
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastReflected * 2f));
            }

            Gizmos.color = isGhost ? Color.green : Color.yellow;
            Gizmos.DrawSphere(origin, 0.08f);
        }
    }
}

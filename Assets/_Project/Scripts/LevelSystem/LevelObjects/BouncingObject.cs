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
        private float _lastBallRadius;
        private Vector2 _lastBallCenter;
        private float _lastOtherRadius;
        private Vector2 _lastOtherCenter;
        private bool _lastOtherIsCircle;

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

        protected virtual Vector2 GetBounceNormal(Collision2D collision)
        {
            var averageNormal = Vector2.zero;
            foreach (var contact in collision.contacts)
                averageNormal += contact.normal;
            return (averageNormal / collision.contactCount).normalized;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!other.enabled) return;
            if (!other.gameObject.TryGetComponent(out BounceObject bounceObject)) return;

            var averageNormal = GetBounceNormal(other);

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

#if DEBUG_LOG
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

                var myCircle = GetComponent<CircleCollider2D>();
                if (myCircle)
                {
                    _lastBallRadius = myCircle.radius * Mathf.Abs(transform.lossyScale.x);
                    _lastBallCenter = (Vector2)transform.TransformPoint(myCircle.offset);
                }

                _lastOtherIsCircle = other.collider is CircleCollider2D;
                if (_lastOtherIsCircle)
                {
                    var otherCircle = (CircleCollider2D)other.collider;
                    _lastOtherRadius = otherCircle.radius * Mathf.Abs(otherCircle.transform.lossyScale.x);
                    _lastOtherCenter = (Vector2)otherCircle.transform.TransformPoint(otherCircle.offset);
                }
            }

            if (!isGhost)
                Debug.Log($"Bounce normal={averageNormal} incoming={_currentVelocity.normalized} reflected={newDirection} speed={speed}");

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
                Gizmos.DrawCube(origin + (Vector3)(_lastNormal * 2f), Vector3.one * 0.1f);
                Gizmos.color = new Color(0.5f, 1f, 0.5f);
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastIncoming * 2f));
                Gizmos.color = new Color(0f, 1f, 0.5f);
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastReflected * 2f));
                Gizmos.DrawSphere(origin + (Vector3)(_lastReflected * 2f), 0.05f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastNormal * 2f));
                Gizmos.DrawCube(origin + (Vector3)(_lastNormal * 2f), Vector3.one * 0.1f);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastIncoming * 2f));
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, origin + (Vector3)(_lastReflected * 2f));
                Gizmos.DrawSphere(origin + (Vector3)(_lastReflected * 2f), 0.05f);
            }

            Gizmos.color = isGhost ? Color.green : Color.yellow;
            Gizmos.DrawSphere(origin, 0.08f);

            if (_lastBallRadius > 0)
            {
                Gizmos.color = isGhost ? new Color(0f, 1f, 0f, 0.3f) : new Color(1f, 1f, 0f, 0.3f);
                DrawGizmoCircle((Vector3)_lastBallCenter, _lastBallRadius, 24);
            }

            if (_lastOtherIsCircle && _lastOtherRadius > 0)
            {
                Gizmos.color = isGhost ? new Color(0f, 0.8f, 0f, 0.3f) : new Color(1f, 0f, 0f, 0.3f);
                DrawGizmoCircle((Vector3)_lastOtherCenter, _lastOtherRadius, 24);
            }
        }

        private static void DrawGizmoCircle(Vector3 center, float radius, int segments)
        {
            float step = 360f / segments;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = step * i * Mathf.Deg2Rad;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
#endif
        }
    }
}

using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class BouncingObject : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private Vector2 _currentVelocity;
        public Vector2 CurrentVelocity => _currentVelocity;

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

            var direction = _currentVelocity.normalized;

            var averageNormal = new Vector2(0, 0);
            foreach (var contact in other.contacts)
            {
                averageNormal += contact.normal;
            }

            averageNormal /= other.contactCount;
            var newDirection = Vector2.Reflect(direction, averageNormal);

            float speed = _currentVelocity.magnitude;

            if (other.rigidbody)
            {
                float relativeApproach = Mathf.Abs(Vector2.Dot(other.relativeVelocity, averageNormal));
                speed = Mathf.Max(speed, relativeApproach);
            }

            _rigidbody2D.linearVelocity
                = newDirection * speed * bounceObject.Bounciness;

        }
    }
}
using System;
using Scripts.Audio;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class BouncingObject : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private Vector2 _currentVelocity;

        private void FixedUpdate()
        {
            _currentVelocity = _rigidbody2D.velocity;
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

            _rigidbody2D.velocity
                = newDirection * _currentVelocity.magnitude * bounceObject.Bounciness;
        }
    }
}
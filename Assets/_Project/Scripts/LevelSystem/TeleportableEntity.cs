using System;
using System.Collections;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class TeleportableEntity : MonoBehaviour
    {
        [SerializeField] private float _teleportCooldown;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private bool _canTeleport = true;
        public bool CanTeleport() => _canTeleport;
        public Action Teleported;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent(out Portal portal))
            {
                portal.Teleport(collision, this);
            }
        }

        public void Teleport(Vector3 position, Vector2 velocity)
        {
            Teleported?.Invoke();
            SetVelocity(velocity);
            SetPosition(position);
        }

        public void DisableTeleporting()
        {
            _canTeleport = false;
        }

        public void EnableTeleporting()
        {
            StartCoroutine(EnableTeleportingCoroutine());
        }

        public IEnumerator EnableTeleportingCoroutine()
        {
            yield return new WaitForSeconds(_teleportCooldown);
            _canTeleport = true;
        }

        public Vector2 GetVelocity()
        {
            return _rigidbody2D.velocity;
        }

        private void SetVelocity(Vector2 velocity)
        {
            _rigidbody2D.velocity = velocity;
        }

        private void SetPosition(Vector3 newPosition)
        {
            transform.position = newPosition;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}
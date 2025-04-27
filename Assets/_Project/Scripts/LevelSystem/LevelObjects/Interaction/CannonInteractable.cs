using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects.Interaction
{
    public class CannonInteractable : MonoBehaviour
    {
        protected Rigidbody2D _rb;
        protected Collider2D _collider;
        protected Vector2 _storedVelocity;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        public virtual void OnEnterCannon()
        {
            if (_rb != null)
            {
                _storedVelocity = _rb.velocity;
                _rb.velocity = Vector2.zero;
                _rb.isKinematic = true;
            }

            if (_collider != null)
                _collider.enabled = false;

            gameObject.SetActive(false);
        }

        public virtual void OnExitCannon(Vector2 shootDirection, float shootForce)
        {
            gameObject.SetActive(true);

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.velocity = shootDirection.normalized * shootForce;
            }

            if (_collider != null)
                _collider.enabled = true;
        }
    }
}

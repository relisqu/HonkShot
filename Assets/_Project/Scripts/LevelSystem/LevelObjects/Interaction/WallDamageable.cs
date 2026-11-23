using System;
using Scripts.Health;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects.Interaction
{
    public class WallDamageable : MonoBehaviour
    {
        [SerializeField] private float _wallDamage;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out LevelSolidObject levelSolidObject))
            {
                var hitPoint = other.contactCount > 0 ? other.GetContact(0).point : (Vector2)other.transform.position;
                GetComponent<HealthController>().TakeDamage(_wallDamage, DamageSource.At(hitPoint));
            }
        }
    }
}
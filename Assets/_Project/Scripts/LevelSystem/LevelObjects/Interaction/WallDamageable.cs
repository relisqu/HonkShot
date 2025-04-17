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
                GetComponent<HealthController>().TakeDamage(_wallDamage);
            }
        }
    }
}
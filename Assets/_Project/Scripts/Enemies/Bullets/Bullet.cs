using System;
using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Enemies.Bullets
{
    public class Bullet : BaseBullet
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerHealth health))
            {
                DamagePlayer(health);
            }
            else if (other.gameObject.TryGetComponent(out BounceObject bounce))
            {
                Destroy(gameObject);
            }
        }
    }
}
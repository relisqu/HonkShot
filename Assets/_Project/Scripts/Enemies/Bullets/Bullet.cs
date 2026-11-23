using Scripts.Other;
using UnityEngine;

namespace Scripts.Enemies.Bullets
{
    public class Bullet : BaseBullet
    {
        protected override void Start()
        {
            base.Start();
            OnReady?.Invoke();
        }

        // Trigger zones (doors, pickups, holes, portals) don't stop bullets; only the player or solid obstacles do.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponentInBody(out PlayerHealth health))
            {
                DamagePlayer(health);
                return;
            }

            if (!other.isTrigger)
                Destroy(gameObject);
        }
    }
}

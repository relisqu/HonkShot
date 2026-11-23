using Scripts.Other;
using System;
using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Enemies.Bullets
{
    public class Bullet : BaseBullet
    {
        public void Start()
        {
            OnReady?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponentInBody(out PlayerHealth health))
            {
                DamagePlayer(health);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
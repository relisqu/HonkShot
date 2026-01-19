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
            if (other.gameObject.TryGetComponent(out PlayerHealth health))
            {
                DamagePlayer(health);
            }
            else
            {
                Debug.Log(other.name);
                Destroy(gameObject);
            }
        }
    }
}
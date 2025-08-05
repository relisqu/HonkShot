using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Enemies;
using UnityEngine;
using Scripts.Health;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class EnemyDamageObject : MonoBehaviour
    {
        [Header("Fire Settings")] [SerializeField]
        private float _damageAmount = 3f;

        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private float _damageIFrame = 0.2f;

        private Dictionary<HealthController, float> _damagedEnemies = new Dictionary<HealthController, float>();
        private float _elapsedTime = 0f;

        private void OnEnable()
        {
            StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(_lifetime);
            ReturnToPool();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                TryDamageEnemy(enemyHealth.HealthController);
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                TryDamageEnemy(enemyHealth.HealthController);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                TryDamageEnemy(enemyHealth.HealthController);
            }
        }


        private void TryDamageEnemy(HealthController enemy)
        {
            float currentTime = Time.time;

            // Check if we can damage this enemy (iframes)
            if (_damagedEnemies.TryGetValue(enemy, out float lastDamageTime))
            {
                if (currentTime - lastDamageTime < _damageIFrame)
                {
                    return;
                }
            }

            enemy.TakeDamage(_damageAmount);
            _damagedEnemies[enemy] = currentTime;
        }


        private void ReturnToPool()
        {
            transform.DOScale(0f, 0.2f).OnComplete(() => { gameObject.SetActive(false); });
        }

        public void Init()
        {
            StartCoroutine(LifetimeRoutine());
        }
    }
}
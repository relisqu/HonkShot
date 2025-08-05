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
    public class EnemySlowObject : MonoBehaviour
    {
        [Header("Fire Settings")] [SerializeField]
        private float SlowSpeed = 3f;

        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private float _damageIFrame = 0.2f;

        private Dictionary<Rigidbody2D, float> _damagedEnemies = new Dictionary<Rigidbody2D, float>();
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
            if (other.gameObject.TryGetComponent(out Rigidbody2D rigidbody2D) &&
                other.gameObject.TryGetComponent(out EnemyHealth _))
            {
                TrySlowEnemy(rigidbody2D);
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out Rigidbody2D rigidbody2D) &&
                other.gameObject.TryGetComponent(out EnemyHealth _))
            {
                TrySlowEnemy(rigidbody2D);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out Rigidbody2D rigidbody2D) &&
                other.gameObject.TryGetComponent(out EnemyHealth _))
            {
                TrySlowEnemy(rigidbody2D);
            }
        }


        private void TrySlowEnemy(Rigidbody2D rigidbody2D)
        {
            float currentTime = Time.time;

            // Check if we can damage this enemy (iframes)
            if (_damagedEnemies.TryGetValue(rigidbody2D, out float lastDamageTime))
            {
                if (currentTime - lastDamageTime < _damageIFrame)
                {
                    return;
                }
            }
            _damagedEnemies[rigidbody2D] = currentTime;
            rigidbody2D.AddForce(-rigidbody2D.velocity * SlowSpeed);
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
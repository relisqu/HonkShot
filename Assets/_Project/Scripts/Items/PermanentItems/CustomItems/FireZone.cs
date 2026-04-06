using System;
using System.Collections;
using Scripts.Enemies;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class FireZone : MonoBehaviour
    {
        [SerializeField] private float _radius = 2f;
        [SerializeField] private float _tickInterval = 0.5f;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private float _fadeInTime = 0.3f;
        [SerializeField] private float _fadeOutTime = 0.5f;

        private float _damagePerSecond;
        private float _duration;
        private float _defaultEmission = -1f;

        public void Init(float damagePerSecond, float duration, float radius, float tickInterval
       )
        {
            _radius = radius;
            _tickInterval = tickInterval;
            Init(damagePerSecond, duration);
        }

        public void Init(float damagePerSecond, float duration)
        {
            _damagePerSecond = damagePerSecond;
            _duration = duration;

            StopAllCoroutines();

            if (_particleSystem)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                CacheDefaultEmission();
                var shape = _particleSystem.shape;
                shape.radius = _radius;
                var main = _particleSystem.main;
                main.duration = duration;
                var emission = _particleSystem.emission;
                emission.rateOverDistanceMultiplier = _radius;
                _particleSystem.Play();
                StartCoroutine(FadeEmissionRoutine());
            }

            StartCoroutine(BurnRoutine());
        }

        private IEnumerator FadeEmissionRoutine()
        {
            var emission = _particleSystem.emission;
            float maxRate = _defaultEmission;

            float t = 0f;
            while (t < _fadeInTime)
            {
                t += Time.deltaTime;
                emission.rateOverTimeMultiplier = Mathf.Lerp(0f,_defaultEmission, t / _fadeInTime);
                yield return null;
            }
            emission.rateOverTimeMultiplier = _defaultEmission;

            float waitTime = Mathf.Max(0f, _duration - _fadeInTime - _fadeOutTime);
            yield return new WaitForSeconds(waitTime);

            t = 0f;
            while (t < _fadeOutTime)
            {
                t += Time.deltaTime;
                emission.rateOverTimeMultiplier = Mathf.Lerp(_defaultEmission, 0f, t / _fadeOutTime);
                yield return null;
            }
            emission.rateOverTimeMultiplier = 0f;
        }

        private IEnumerator BurnRoutine()
        {
            float elapsed = 0f;
            float damagePerTick = _damagePerSecond * _tickInterval;

            while (elapsed < _duration)
            {
                DealDamage(damagePerTick);
                yield return new WaitForSeconds(_tickInterval);
                elapsed += _tickInterval;
            }

            gameObject.SetActive(false);
        }

        private void DealDamage(float damage)
        {
            if (!ExplosionSystem.Instance) return;

            var hits = Physics2D.OverlapCircleAll(transform.position, _radius, ExplosionSystem.Instance.EnemyLayerMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out EnemyHealth enemyHealth))
                    enemyHealth.HealthController.TakeDamage(damage);
            }
        }

        private void OnDisable()
        {
            if (_particleSystem)
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            StopAllCoroutines();
        }

        private void CacheDefaultEmission()
        {
            if (_defaultEmission < 0f && _particleSystem)
                _defaultEmission = _particleSystem.emission.rateOverTime.constant;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}

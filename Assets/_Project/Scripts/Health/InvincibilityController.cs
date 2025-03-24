using System;
using System.Collections;
using UnityEngine;

namespace Scripts.Health
{
    [RequireComponent(typeof(HealthController))]
    public class InvincibilityController : MonoBehaviour
    {
        private HealthController _healthController;
        [SerializeField] private float _iFrameDuration;

        private void HealthController_Damaged()
        {
            StartInvincibility(_iFrameDuration);
        }

        public void StartInvincibility(float invincibilityDuration)
        {
            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
        }

        private IEnumerator InvincibilityCoroutine(float invincibilityDuration)
        {
            _healthController.SetInvincible(true);
            yield return new WaitForSeconds(invincibilityDuration);
            _healthController.SetInvincible(false);
        }

        private void Awake()
        {
            _healthController = GetComponent<HealthController>();
            _healthController.OnDamaged += HealthController_Damaged;
        }

        private void OnDestroy()
        {
            _healthController.OnDamaged -= HealthController_Damaged;
        }
    }
}
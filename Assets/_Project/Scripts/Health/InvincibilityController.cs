using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Health
{
    public class InvincibilityController : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private float _iFrameDuration;
        public float IrameDuration => _iFrameDuration;

        private void HealthController_OnNonLethalDamageReceived(float damage)
        {
            StartInvincibility(_iFrameDuration);
        }

        private void HealthController_OnDamageBlocked(float damage)
        {
            StartInvincibility(_iFrameDuration);
        }

        public void StartInvincibility(float invincibilityDuration)
        {
            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
        }

        private IEnumerator InvincibilityCoroutine(float invincibilityDuration)
        {
            _healthController.SetInvincible((int)InvincibilityEnum.IFrames, true);
            yield return new WaitForSeconds(invincibilityDuration);
            _healthController.SetInvincible((int)InvincibilityEnum.IFrames, false);
        }

        private void Awake()
        {
            if(_healthController == null)
            _healthController = GetComponent<HealthController>();
            _healthController.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
            _healthController.OnDamageBlocked += HealthController_OnDamageBlocked;
        }

        private void OnDestroy()
        {
            _healthController.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
            _healthController.OnDamageBlocked -= HealthController_OnDamageBlocked;
        }
    }
}
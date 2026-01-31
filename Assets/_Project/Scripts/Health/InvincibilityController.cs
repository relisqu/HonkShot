using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Health
{
    [RequireComponent(typeof(HealthController))]
    public class InvincibilityController : MonoBehaviour
    {
        private HealthController _healthController;
        [SerializeField] private float _iFrameDuration;
        public float IrameDuration => _iFrameDuration;

        private void HealthController_OnNonLethalDamageReceived(float damage)
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
            _healthController = GetComponent<HealthController>();
            _healthController.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
        }

        private void OnDestroy()
        {
            _healthController.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
        }
    }
}
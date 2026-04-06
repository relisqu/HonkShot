using System;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class EnemyArmor : MonoBehaviour
    {
        [Header("Armor Settings")]
        [SerializeField] private int _armorHits = 3;
        [SerializeField] private float _damageThreshold = 5f;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _armorVisual;

        private int _remainingHits;
        private bool _armorDestroyed;

        public bool IsArmorActive => !_armorDestroyed && _remainingHits > 0;
        public int RemainingHits => _remainingHits;
        public event Action OnArmorHit;
        public event Action OnArmorDestroyed;

        private void Awake()
        {
            _remainingHits = _armorHits;
        }

        /// <summary>
        /// Called by EnemyHealth BEFORE any damage reaches HealthController.
        /// Returns true if the armor absorbed the damage (the enemy takes no health loss).
        /// Returns false if the armor is broken or inactive and damage should pass through.
        /// </summary>
        public bool TryAbsorbDamage(float damage)
        {
            if (_armorDestroyed) return false;

            // Small hits bounce off harmlessly — no health loss, no armor hit counted.
            if (damage < _damageThreshold) return true;

            // Qualifying hit: armor absorbs the damage and loses one charge.
            _remainingHits--;
            OnArmorHit?.Invoke();

            if (_remainingHits <= 0)
                DestroyArmor();
            else
                UpdateVisual();

            return true;
        }

        private void DestroyArmor()
        {
            _armorDestroyed = true;

            if (_armorVisual)
                _armorVisual.enabled = false;

            OnArmorDestroyed?.Invoke();
        }

        private void UpdateVisual()
        {
            if (!_armorVisual || _armorDestroyed) return;

            float ratio = (float)_remainingHits / _armorHits;
            Color color = _armorVisual.color;
            color.a = Mathf.Lerp(0.3f, 1f, ratio);
            _armorVisual.color = color;
        }
    }
}

using System;
using Scripts.Player;
using Scripts.Items.StatSystems;
using UnityEngine;
using Zenject;

namespace Scripts.Health
{
    public class PlayerAttackController : AttackController
    {
        [Header("References")]
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private GooseFireSystem _gooseFireSystem;

        [Header("Damage Formula Settings")]
        [SerializeField] private float _maxSpeed = 15f;
        [SerializeField] private float _minFireMultiplier = 1f;
        [SerializeField] private float _maxFireMultiplier = 2f;
        [SerializeField] private float _minDamage = 5f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = false;

        private NumericStatModifierSystem _damageBuffModifierSystem = new();

        public NumericStatModifierSystem DamageBuffModifierSystem => _damageBuffModifierSystem;
        public Action<GameObject> OnHit;
        public Action<float> OnDamaged;

        private void Awake()
        {
            if (!_gooseFireSystem)
                _gooseFireSystem = GetComponentInParent<GooseFireSystem>();
            if (!_playerBallMovement)
                _playerBallMovement = GetComponentInParent<PlayerBallMovement>();
        }

        public void Damage(GameObject target)
        {
            OnHit?.Invoke(target);
        }

        public override float GetDamage()
        {
            float currentSpeed = _playerBallMovement ? _playerBallMovement.CurrentSpeed : 0f;
            float speedMapped = Mathf.Clamp(currentSpeed, 0f, _maxSpeed);

            float currentFire = _gooseFireSystem ? _gooseFireSystem.Fire : 0f;
            float maxFire = _gooseFireSystem ? _gooseFireSystem.MaxFire : 100f;
            float fireNormalized = Mathf.Clamp01(currentFire / maxFire);
            float fireMapped = Mathf.Lerp(_minFireMultiplier, _maxFireMultiplier, fireNormalized);

            float baseDamage = speedMapped * fireMapped + _minDamage;

            float buffsDamage = _damageBuffModifierSystem.Calculate(0f);
            float finalDamage = baseDamage * (1f + buffsDamage);

            if (_debugMode)
            {
                Debug.Log($"[PlayerDamage] Speed: {currentSpeed:F1}/{_maxSpeed} (mapped: {speedMapped:F1}) | " +
                          $"Fire: {currentFire:F0}/{maxFire:F0} (mult: {fireMapped:F2}) | " +
                          $"Base: {baseDamage:F1} | Buffs: {buffsDamage:F2} | Final: {finalDamage:F1}");
            }

            return finalDamage;
        }

        public float GetFlatDamage(float amount)
        {
            float buffsDamage = _damageBuffModifierSystem.Calculate(0f);
            float finalDamage = amount * (1f + buffsDamage);

            if (_debugMode)
            {
                Debug.Log($"[PlayerDamage] Flat: {amount:F1} | Buffs: {buffsDamage:F2} | Final: {finalDamage:F1}");
            }

            return finalDamage;
        }

        public float GetRawFlatDamage(float amount)
        {
            if (_debugMode)
            {
                Debug.Log($"[PlayerDamage] Raw Flat: {amount:F1}");
            }

            return amount;
        }

        public float GetSpeedComponent()
        {
            float currentSpeed = _playerBallMovement ? _playerBallMovement.CurrentSpeed : 0f;
            return Mathf.Clamp(currentSpeed, 0f, _maxSpeed);
        }

        public float GetFireMultiplier()
        {
            if (!_gooseFireSystem) return _minFireMultiplier;

            float fireNormalized = Mathf.Clamp01(_gooseFireSystem.Fire / _gooseFireSystem.MaxFire);
            return Mathf.Lerp(_minFireMultiplier, _maxFireMultiplier, fireNormalized);
        }
    }
}
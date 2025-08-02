using System;
using Scripts.Player;
using Scripts.PointSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Scripts.Items;

namespace Scripts.Health
{
    public class PlayerAttackController : AttackController
    {
        [Header("References")] [SerializeField]
        private PlayerBallMovement _playerBallMovement;

        [Header("Speed")] [SerializeField] private bool _dependsOnSpeed;

        [ShowIf("_dependsOnSpeed")] [SerializeField]
        private float _minBonusCoeffSpeed;

        [ShowIf("_dependsOnSpeed")] [SerializeField]
        private float _maxBonusCoeffSpeed;

        [ShowIf("_dependsOnSpeed")] [SerializeField]
        private float _maxSpeedCoeff = 2f;

        private NumericStatModifierSystem _numericStatModifierSystem = new();

        public NumericStatModifierSystem NumericStatModifierSystem => _numericStatModifierSystem;
        public Action<GameObject> OnHit;


        public float CalculateDamage(float baseDamage)
        {
            float result = _numericStatModifierSystem.Calculate(baseDamage);

            Debug.Log($"Damage:: {result}");
            return result;
        }

        public void Damage(GameObject target)
        {
            OnHit?.Invoke(target);
        }

        public override float GetDamage()
        {
            Debug.Log("Touch speed: " + _playerBallMovement.CurrentSpeed);

            var damage = PointReceiver.Instance.GetBaseDamage();
            var attackDamage = PointReceiver.Instance.GetAttackPoints();
            if (_dependsOnSpeed)
            {
                var currentSpeed = _playerBallMovement.CurrentSpeed;
                var currentSpeedValue = Mathf.InverseLerp(_minBonusCoeffSpeed, _maxBonusCoeffSpeed, currentSpeed);

                Debug.Log("currentSpeedValue: " + currentSpeedValue);

                var bonusSpeedCoeff = Mathf.Max(1, Mathf.Lerp(1, _maxSpeedCoeff, currentSpeedValue));
                attackDamage *= bonusSpeedCoeff;

                Debug.Log("AppliedCoeff: " + bonusSpeedCoeff);
            }

            float baseTotal = damage + attackDamage;
            return CalculateDamage(baseTotal);
        }

        public float GetSpeedModifier()
        {
            var currentSpeed = _playerBallMovement.CurrentSpeed;
            if (currentSpeed < _minBonusCoeffSpeed)
                return 0;
            var currentSpeedValue = Mathf.InverseLerp(_minBonusCoeffSpeed, _maxBonusCoeffSpeed, currentSpeed);

            var bonusSpeedCoeff = Mathf.Lerp(1, _maxSpeedCoeff, currentSpeedValue) - 1;
            return bonusSpeedCoeff;
        }
    }
}
using System;
using System.Collections.Generic;
using Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.PointSystem
{
    public class PointReceiver : MonoBehaviour
    {
        public static PointReceiver Instance;

        [SerializeField] private float _perTypeCoefficient = 1f;
        [SerializeField] private float _baseDamage;

        private int _maxPointsCount;
        private float _currentPointsAttack;
        public Action ChangedCurrentPoints;
        public Action ChangedMaxPoints;
        private int CurrentPoints => (int)_currentPointsAttack;

        public int MaxPointsCount => _maxPointsCount;

        private Dictionary<AttackPointsObjectType, int> _pointObjectTypes =
            new Dictionary<AttackPointsObjectType, int>();

        private void Awake()
        {
            Instance = this;
        }


        public float AveragePointsPerBounce => _currentPointsAttack / _bouncesCount;

        private int _bouncesCount = 0;

        public void GeneratePointsParticle(int points)
        {
            var textParticle = new UIFactory().CreateUITextParticle(transform.position);
            textParticle.ShowText(Mathf.Sign(points) > 0 ? $"+{points}" : $"-{points}");
            textParticle.SetScale(points / AveragePointsPerBounce);
        }

        public void GenerateFirePointParticle()
        {
            var particle = new UIFactory().CreateUIFireParticle(transform.position);
            particle.Show();
        }

        public void AddPoints(AttackPointsObjectType attackPointsObjectType, float points)
        {
            _currentPointsAttack += points;
            _bouncesCount++;
            ChangedCurrentPoints?.Invoke();
            GeneratePointsParticle((int)points);

            if (attackPointsObjectType == AttackPointsObjectType.None)
                return;
            if (!_pointObjectTypes.TryAdd(attackPointsObjectType, 1))
            {
                _pointObjectTypes[attackPointsObjectType]++;
            }
            else
            {
                GenerateFirePointParticle();
            }
        }

        public void AddMaxPoints()
        {
            var intPoints = Mathf.RoundToInt(GetAttackPoints() - _baseDamage);
            if (intPoints <= 0)
            {
                return;
            }

            Debug.Log(_maxPointsCount);
            _maxPointsCount += intPoints;
            ChangedMaxPoints?.Invoke();
        }

        public float GetAttackPoints()
        {
            var coeff = 1 + _pointObjectTypes.Count * _perTypeCoefficient;
            var damage = _baseDamage + _currentPointsAttack * coeff;
            return damage;
        }

        public void ResetAttack()
        {
            _bouncesCount = 0;
            _currentPointsAttack = 0;
            ChangedCurrentPoints?.Invoke();
            _pointObjectTypes.Clear();
        }
    }
}
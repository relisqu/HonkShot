using System;
using System.Collections.Generic;
using Scripts.Audio;
using Scripts.Items;
using Scripts.Items.StatSystems;
using Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Scripts.PointSystem
{
    public class PointReceiver : MonoBehaviour
    {
        // Keep Instance for backward compatibility, but prefer injection
        public static PointReceiver Instance { get; private set; }

        [SerializeField] private float _perTypeCoefficient = 1f;
        [SerializeField] private float _baseDamage;

        private int _maxPointsCount;
        private float _currentPointsAttack;
        private int CurrentPoints => (int)_currentPointsAttack;
        private NumericStatModifierSystem _pointReceiveModifierSystem = new();
        private int _bouncesCount = 0;

        public Action ChangedCurrentPoints;
        public Action ChangedMaxPoints;

        private Dictionary<AttackPointsObjectType, int> _pointObjectTypes =
            new();

        public int MaxPointsCount => _maxPointsCount;
        public NumericStatModifierSystem PointReceiveModifierSystem => _pointReceiveModifierSystem;
        public float AveragePointsPerBounce => _maxPointsCount * 1f / _bouncesCount;

        [Inject]
        private void Construct()
        {
            // Zenject injection point
        }

        private void Awake()
        {
            Instance = this; // Keep for backward compatibility
        }


        public void GeneratePointsParticle(int points)
        {
            var textParticle = new UIFactory().CreateUITextParticle(transform.position);
            textParticle.ShowText(Mathf.Sign(points) >= 0 ? $"+{points}" : $"-{points}");
            Debug.Log(AveragePointsPerBounce);
            if (AveragePointsPerBounce <= 0)
            {
                textParticle.SetScale(1f);
            }
            else
            {
                textParticle.SetScale(points / AveragePointsPerBounce);
            }

            AudioManager.Instance.PlayOneShot(SoundChanelType.UI, "scorePoint", points / AveragePointsPerBounce);
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
            GeneratePointsParticle((int)(points));

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
            var intPoints = Mathf.RoundToInt(_pointReceiveModifierSystem.Calculate(GetAttackPoints()));
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
            return coeff * _currentPointsAttack;
        }

        public float GetBaseDamage()
        {
            return _baseDamage;
        }

        public void ResetCurrentAttack()
        {
            _bouncesCount = 0;
            _currentPointsAttack = 0;
            ChangedCurrentPoints?.Invoke();
            _pointObjectTypes.Clear();
        }

        public void ResetPoints()
        {
            _maxPointsCount = 0;
        }
    }
}
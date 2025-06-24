using System;
using System.Collections.Generic;
using Scripts.Audio;
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

        private Dictionary<string, float> _multipliers = new Dictionary<string, float>();

        public float PointsMultiplier
        {
            get
            {
                float result = 1f;
                foreach (var m in _multipliers.Values)
                    result *= m;
                return result;
            }
        }

        public void AddMultiplier(string key, float value)
        {
            _multipliers[key] = value;
        }

        public void RemoveMultiplier(string key)
        {
            _multipliers.Remove(key);
        }

        private void Awake()
        {
            Instance = this;
        }


        public float AveragePointsPerBounce => _maxPointsCount * 1f / _bouncesCount;

        private int _bouncesCount = 0;

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
            GeneratePointsParticle((int)(points*PointsMultiplier));

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
            var intPoints = Mathf.RoundToInt(GetAttackPoints());
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
            return coeff * PointsMultiplier* _currentPointsAttack;
        }

        public float GetBaseDamage()
        {
            return _baseDamage;
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
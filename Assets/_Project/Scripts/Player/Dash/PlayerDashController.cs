using System;
using System.Collections;
using Scripts.Items;
using Scripts.Items.StatSystems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player.Dash
{
    public class PlayerDashController : MonoBehaviour
    {
        [FormerlySerializedAs("_dashCount")] [SerializeField]
        private int _maxDashCount;


        [SerializeField] private bool _canRegenDashes = true;

        [Tooltip("How many dashes player regenerates per second")] [SerializeField]
        private float _dashRegenerationRate;

        private int _currentDashCount;

        public Action UpdatedDashCount;
        public Action UpdatedMaxDashCount;
        public Action OnDashUsed;
        public int CurrentDashCount => _currentDashCount;
        public bool CanDash => _currentDashCount > 0;
        private Coroutine _regenerateDashCoroutine;
        public int MaxDashCount => GetMaxDashCount();
        private NumericStatModifierSystem _maxDashModifierSystem = new();
        public NumericStatModifierSystem MaxDashModifierSystem => _maxDashModifierSystem;

        private NumericStatModifierSystem _dashCooldownModifierSystem = new();
        public NumericStatModifierSystem DashCooldownModifierSystem => _dashCooldownModifierSystem;

        private void Start()
        {
            _currentDashCount = MaxDashCount;
            UpdatedDashCount?.Invoke();
            _regenerateDashCoroutine = StartCoroutine(RegenerateDashes());
        }

        public void OnDestroy()
        {
            StopCoroutine(_regenerateDashCoroutine);
        }

        public int GetMaxDashCount()
        {
            return (int)_maxDashModifierSystem.Calculate(_maxDashCount);
        }

        public float GetDashRegenerationRate()
        {
            return _dashCooldownModifierSystem.Calculate(1 / _dashRegenerationRate);
        }

        public IEnumerator RegenerateDashes()
        {
            while (true)
            {
                while (!_canRegenDashes)
                {
                    yield return null;
                }

                if (_currentDashCount < MaxDashCount)
                {
                    AddDash();
                }

                yield return new WaitForSeconds(GetDashRegenerationRate());
            }

            yield return null;
        }

        public void AddDash()
        {
            _currentDashCount++;
            _currentDashCount = Mathf.Min(_currentDashCount, _maxDashCount);
            Debug.Log($"New dash added: {_currentDashCount}");
            UpdatedDashCount?.Invoke();
        }

        public void AddMaxDashes(int dashCount)
        {
            _maxDashCount += dashCount;
            _maxDashCount = Mathf.Clamp(_maxDashCount, 0, _maxDashCount);
            Debug.Log($"Changed max dashes count, current: {_maxDashCount}");
            UpdatedMaxDashCount?.Invoke();
            UpdatedDashCount?.Invoke();
        }

        public void ResetDashes()
        {
            _currentDashCount = MaxDashCount;
            UpdatedDashCount?.Invoke();
            UpdatedMaxDashCount?.Invoke();
        }

        public void DecreaseDashCount()
        {
            Debug.Log($"Trying remove dash");
            if (_currentDashCount <= 0) return;
            Debug.Log($"Dash removed: {_currentDashCount}");
            _currentDashCount--;
            UpdatedDashCount?.Invoke();
            OnDashUsed?.Invoke();
        }
    }
}
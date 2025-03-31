using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player.Dash
{
    public class PlayerDashController : MonoBehaviour
    {
        [FormerlySerializedAs("_dashCount")] [SerializeField]
        private int _maxDashCount;


        [SerializeField] private bool _canRegenDashes = true;
        [Tooltip("How many dashes player regenerates per second")][SerializeField] private float _dashRegenerationRate;

        private int _currentDashCount;

        public Action UpdatedDashCount;
        public int CurrentDashCount => _currentDashCount;
        public bool CanDash => _currentDashCount > 0;
        private Coroutine _regenerateDashCoroutine;
        public int MaxDashCount => _maxDashCount;

        private void Start()
        {
            _currentDashCount = _maxDashCount;
            UpdatedDashCount?.Invoke();
            _regenerateDashCoroutine = StartCoroutine(RegenerateDashes());
        }

        public void OnDestroy()
        {
            StopCoroutine(_regenerateDashCoroutine);
        }

        public IEnumerator RegenerateDashes()
        {
            while (true)
            {
                while (!_canRegenDashes)
                {
                    yield return null;
                }

                if (_currentDashCount < _maxDashCount)
                {
                    AddDash();
                    yield return new WaitForSeconds(1 / _dashRegenerationRate);
                }

                yield return null;
            }

            yield return null;
        }

        public void AddDash()
        {
            _currentDashCount++;
            Debug.Log($"New dash added: {_currentDashCount}");
            UpdatedDashCount?.Invoke();
        }

        public void ResetDashes()
        {
            _currentDashCount = _maxDashCount;
            UpdatedDashCount?.Invoke();
        }

        public void DecreaseDashCount()
        {
            Debug.Log($"Trying remove dash");
            if (_currentDashCount <= 0) return;
            Debug.Log($"Dash removed: {_currentDashCount}");
            _currentDashCount--;
            UpdatedDashCount?.Invoke();
        }
    }
}
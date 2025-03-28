using System;
using Scripts.Player;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class TouchableObject : MonoBehaviour
    {
        [SerializeField] private int _maxTouchCountPerDash;
        private int _currentTouchCount;

        public Action<int> Touched;

        public Action Resetted;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerMovement playerMovement))
            {
                if (_maxTouchCountPerDash == -1 || _currentTouchCount < _maxTouchCountPerDash)
                {
                    Touch();
                }
            }
        }

        private void Touch()
        {
            _currentTouchCount++;
            Debug.Log(_currentTouchCount);
            Touched?.Invoke(_currentTouchCount);
        }

        public void Reset()
        {
            _currentTouchCount = 0;
            Resetted?.Invoke();
        }
    }
}
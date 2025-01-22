using System;
using Scripts.Audio;
using Scripts.Camera;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scripts.Player.Stamina
{
    public class StaminaManager : MonoBehaviour
    {
        [FormerlySerializedAs("maxStamina")] [SerializeField]
        private float _maxStamina = 100f;
        [SerializeField] private float _startStamina = 40f;
        [FormerlySerializedAs("regenerationRate")] [SerializeField]
        private float _regenerationRate = 20f;
        [SerializeField] private float _regenerationDelay = 2f;

        private float _currentStamina;
        private float _timeSinceLastStaminaUse;

        public Action OnStaminaValueChanged;
        public Action OnStaminaLow;

        void Start()
        {
            _currentStamina = _startStamina;
            OnStaminaValueChanged?.Invoke();
        }

        void Update()
        {
            RegenerateStamina();
            
            _timeSinceLastStaminaUse += Time.deltaTime;
        }

        public bool TrySpendStamina(float amount)
        {
            if (_currentStamina >= amount)
            {
                _currentStamina -= amount;
                OnStaminaValueChanged?.Invoke();
                _timeSinceLastStaminaUse = 0f;
                return true;
            }

            OnStaminaLow?.Invoke();
            return false;
        }

        private void RegenerateStamina()
        {
            // Regenerate stamina only if the delay has passed
            if (_timeSinceLastStaminaUse >= _regenerationDelay && _currentStamina < _maxStamina)
            {
                _currentStamina += _regenerationRate * Time.deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
                OnStaminaValueChanged?.Invoke();
            }
        }

        public float GetCurrentStamina()
        {
            return _currentStamina;
        }

        public float GetMaxStamina()
        {
            return _maxStamina;
        }
    }
}
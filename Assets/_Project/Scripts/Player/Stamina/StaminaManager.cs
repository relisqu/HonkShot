using Scripts.Audio;
using Scripts.Camera;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scripts.Player.Stamina
{
    public class StaminaManager : MonoBehaviour
    {
        [FormerlySerializedAs("maxStamina")] public float _maxStamina = 100f;
        [FormerlySerializedAs("regenerationRate")] public float _regenerationRate = 10f;
        
        private float _currentStamina;
        
        

        void Start()
        {
            _currentStamina = _maxStamina;
            UpdateStaminaBar();
        }

        void Update()
        {
            RegenerateStamina();
            UpdateStaminaBar();

        }

        public bool TrySpendStamina(float amount)
        {
            if (_currentStamina >= amount)
            {
                _currentStamina -= amount;
                UpdateStaminaBar();
                return true;
            }
            return false;
        }

        private void RegenerateStamina()
        {
            if (_currentStamina < _maxStamina)
            {
                _currentStamina += _regenerationRate * Time.deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
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
using System;
using Scripts.Player.Stamina;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class StaminaBar : MonoBehaviour
    {
        [SerializeField] private Slider _staminaBar;
        [SerializeField] private StaminaManager _staminaManager;

        private void OnEnable()
        {
            _staminaManager.OnStaminaValueChanged += StaminaManager_StaminaValueChange;
        }

        private void OnDisable()
        {
            _staminaManager.OnStaminaValueChanged -= StaminaManager_StaminaValueChange;
        }

        private void StaminaManager_StaminaValueChange()
        {
            UpdateStaminaBar();
        }


        private void UpdateStaminaBar()
        {
            Debug.Log(_staminaBar);
            _staminaBar.value = _staminaManager.GetCurrentStamina() / _staminaManager.GetMaxStamina();
        }
    }
}
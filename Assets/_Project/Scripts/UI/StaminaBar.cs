using Scripts.Player.Stamina;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.UI
{
    public class StaminaBar : MonoBehaviour
    {
        [SerializeField] private Slider _staminaBar;
        [SerializeField] private StaminaManager _staminaManager;

        private void UpdateStaminaBar()
        {
            _staminaBar.value = _staminaManager.GetCurrentStamina() / _staminaManager.GetMaxStamina();
        }
    }
}
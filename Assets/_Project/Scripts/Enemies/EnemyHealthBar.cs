using System.Collections;
using DG.Tweening;
using Scripts.Health;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Enemies
{
    namespace Scripts.UI
    {
        public class EnemyHealthBar : MonoBehaviour
        {
            [SerializeField] private Slider _hpSlider;
            [SerializeField] private CanvasGroup _canvasGroup;
            [SerializeField] private EnemyHealth _enemyHealth;
            [SerializeField] private float _hideDelay = 2f;

            private Coroutine _hideCoroutine;

            private void Start()
            {
                _canvasGroup.alpha = 0;
                _enemyHealth.HealthController.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
                _enemyHealth.HealthController.OnDied += HideHealthBar;
                UpdateHealthBar();
            }

            private void OnDestroy()
            {
                _enemyHealth.HealthController.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
                _enemyHealth.HealthController.OnDied -= HideHealthBar;
            }

            private void HealthController_OnNonLethalDamageReceived(float damage)
            {
                _canvasGroup.DOFade(1f, 0.1f);
                UpdateHealthBar();

                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                }

                _hideCoroutine = StartCoroutine(HideAfterDelay());
            }

            private void HideHealthBar(HealthController controller = null)
            {
                _canvasGroup.DOFade(0f, 0.1f);
            }

            private IEnumerator HideAfterDelay()
            {
                yield return new WaitForSeconds(_hideDelay);
                HideHealthBar();
            }

            private void UpdateHealthBar()
            {
                _hpSlider.value = _enemyHealth.HealthController.RemainingHealthPercentage;
            }
        }
    }
}
using System.Collections;
using DG.Tweening;
using Scripts.Audio;
using Scripts.Camera;
using Scripts.Health;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Enemies
{
    [RequireComponent(typeof(HealthController))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        public HealthController HealthController => _healthController;

        private void HealthController_Died()
        {
            StartCoroutine(DeathSequence());
        }

        public IEnumerator DeathSequence()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player,"takeDamage", 1f);
            CameraShakeHandler.Instance.ShakeCameraOutsideOfQueue(0.4f, 10f);
            yield return new WaitForSeconds(0.2f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HealthController_Damaged()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player,"takeDamage", 0.5f);
            CameraShakeHandler.Instance.ShakeCameraOutsideOfQueue(0.2f, 3f);
        }

        private void Start()
        {
            _healthController.OnDied += HealthController_Died;
            _healthController.OnDamaged += HealthController_Damaged;
        }

        private void OnDestroy()
        {
            _healthController.OnDied -= HealthController_Died;
            _healthController.OnDamaged -= HealthController_Damaged;
        }
    }
}
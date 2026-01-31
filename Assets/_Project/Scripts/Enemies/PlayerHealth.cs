using System;
using System.Collections;
using DG.Tweening;
using Scripts.Audio;
using Scripts.Camera;
using Scripts.Health;
using Scripts.Managers.Tutorial;
using Scripts.Player;
using Scripts.Player.Dash;
using Scripts.PointSystem;
using Scripts.Progress;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Scripts.Enemies
{
    [RequireComponent(typeof(HealthController))]
    public class PlayerHealth : MonoBehaviour
    {
        private ProgressSaver _progressSaver;
        private TutorialController _tutorialController;
        [SerializeField] private HealthController _healthController;
        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private GooseFireSystem _gooseFireSystem;
        public HealthController HealthController => _healthController;


        [Inject]
        private void Construct(ProgressSaver progressSaver, TutorialController tutorialController)
        {
            _progressSaver = progressSaver;
            _tutorialController = tutorialController;
        }

        private void HealthController_Died()
        {
            StartCoroutine(DeathSequence());
        }

        public IEnumerator DeathSequence()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "takeDamage", 1f);
            CameraShakeHandler.Instance.ShakeCameraOutsideOfQueue(0.4f, 10f);
            yield return new WaitForSeconds(0.2f);

            if (_progressSaver.ProgressData.TutorialCompletedStatus == ProgressData.TutorialStatus.InProcess)
            {
                _tutorialController.RestartTutorialStep();
                ResetPlayer();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public void ResetPlayer()
        {
            _healthController.ResetHealthToDefault();
            PointReceiver.Instance.ResetPoints();
            _gooseFireSystem.ResetStats();
            _playerDashController.ResetDashes();
        }

        private void HealthController_OnNonLethalDamageReceived(float damage)
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "takeDamage", 0.5f);
            CameraShakeHandler.Instance.ShakeCameraOutsideOfQueue(0.2f, 3f);
        }

        private void Start()
        {
            _healthController.OnDied += HealthController_Died;
            _healthController.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
        }

        private void OnDestroy()
        {
            _healthController.OnDied -= HealthController_Died;
            _healthController.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
        }
    }
}
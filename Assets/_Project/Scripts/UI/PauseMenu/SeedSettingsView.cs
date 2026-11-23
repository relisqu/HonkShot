using DG.Tweening;
using Scripts.Other;
using Scripts.Services.Seed;
using Scripts.Services.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI.PauseMenu
{
    public class SeedSettingsView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _seedInput;
        [SerializeField] private Button _copyButton;
        [SerializeField] private Button _playButton;

        [Header("Feedback")]
        [SerializeField] private float _punchScale = 0.15f;
        [SerializeField] private float _punchDuration = 0.25f;

        private RunSeedService _runSeedService;
        private SettingsService _settingsService;

        [Inject]
        private void Construct(RunSeedService runSeedService, SettingsService settingsService)
        {
            _runSeedService = runSeedService;
            _settingsService = settingsService;
        }

        private void Awake()
        {
            _copyButton.onClick.AddListener(CopyButton_Click);
            _playButton.onClick.AddListener(PlayButton_Click);
        }

        private void OnDestroy()
        {
            _copyButton.onClick.RemoveListener(CopyButton_Click);
            _playButton.onClick.RemoveListener(PlayButton_Click);
        }

        public void Refresh()
        {
            if (_runSeedService == null) return;
            _seedInput.SetTextWithoutNotify(_runSeedService.CurrentSeedText);
        }

        private void CopyButton_Click()
        {
            string text = string.IsNullOrWhiteSpace(_seedInput.text)
                ? _runSeedService.CurrentSeedText
                : _seedInput.text.Trim();

            GUIUtility.systemCopyBuffer = text;
            Punch(_copyButton.transform);
        }

        private void PlayButton_Click()
        {
            if (!SeedUtility.TryParse(_seedInput.text, out int seed))
            {
                Refresh();
                Punch(_seedInput.transform);
                return;
            }

            _settingsService.Save();
            _runSeedService.SetSeedForNextRun(seed);
            SceneRestarter.RestartCurrentScene();
        }

        private void Punch(Transform target)
        {
            target.DOKill(true);
            target.DOPunchScale(Vector3.one * _punchScale, _punchDuration).SetUpdate(true);
        }
    }
}

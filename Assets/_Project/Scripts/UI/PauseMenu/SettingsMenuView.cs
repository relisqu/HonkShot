using System;
using Scripts.Services.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI.PauseMenu
{
    public class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private VolumeSliderView[] _volumeSliders;
        [SerializeField] private LanguageSelectorView _languageSelector;
        [SerializeField] private Button _backButton;
        [SerializeField] private Selectable _firstSelected;

        private SettingsService _settingsService;

        public event Action OnClosed;
        public bool IsOpen => gameObject.activeSelf;

        [Inject]
        private void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(BackButton_Click);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(BackButton_Click);
        }

        public void Show()
        {
            gameObject.SetActive(true);

            foreach (var volumeSlider in _volumeSliders)
                volumeSlider.Refresh();
            _languageSelector.Refresh();

            if (EventSystem.current && _firstSelected)
                EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
        }

        public void Hide()
        {
            _settingsService.Save();
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        private void BackButton_Click() =>
            Hide();
    }
}

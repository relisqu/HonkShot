using Scripts.Services.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI.PauseMenu
{
    public class LanguageSelectorView : MonoBehaviour
    {
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _languageText;

        private SettingsService _settingsService;

        [Inject]
        private void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private void Awake()
        {
            _previousButton.onClick.AddListener(PreviousButton_Click);
            _nextButton.onClick.AddListener(NextButton_Click);
        }

        private void OnDestroy()
        {
            _previousButton.onClick.RemoveListener(PreviousButton_Click);
            _nextButton.onClick.RemoveListener(NextButton_Click);
        }

        public void Refresh()
        {
            var languages = _settingsService.Languages;
            if (languages.Length == 0) return;

            _languageText.text = languages[GetCurrentLanguageIndex()].Name;
        }

        private void PreviousButton_Click() =>
            ShiftLanguage(-1);

        private void NextButton_Click() =>
            ShiftLanguage(1);

        private void ShiftLanguage(int direction)
        {
            var languages = _settingsService.Languages;
            if (languages.Length == 0) return;

            int index = (GetCurrentLanguageIndex() + direction + languages.Length) % languages.Length;
            _settingsService.SetLanguage(languages[index].Key);
            Refresh();
        }

        private int GetCurrentLanguageIndex()
        {
            var languages = _settingsService.Languages;
            for (int i = 0; i < languages.Length; i++)
                if (languages[i].Key == _settingsService.LanguageKey)
                    return i;

            return 0;
        }
    }
}

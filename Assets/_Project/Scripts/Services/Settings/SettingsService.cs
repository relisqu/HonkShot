using System;
using System.Collections.Generic;
using Scripts.Services.Localization;
using UnityEngine;

namespace Scripts.Services.Settings
{
    public class SettingsService
    {
        private const string SaveKey = "settings";
        private const string LanguageDictionaryPathFormat = "Data/{0}";

        private readonly ILocalizationService _localizationService;
        private readonly LanguagesConfigSO _languagesConfig;
        private readonly SettingsData _data;

        public event Action<VolumeCategory, float> VolumeChanged;

        public Language[] Languages => _languagesConfig.Languages;
        public string LanguageKey => _data.LanguageKey;

        public SettingsService(ILocalizationService localizationService, LanguagesConfigSO languagesConfig)
        {
            _localizationService = localizationService;
            _languagesConfig = languagesConfig;
            _data = ES3.Load(SaveKey, new SettingsData());
        }

        public void Save() =>
            ES3.Save(SaveKey, _data);

        public float GetVolume(VolumeCategory category) =>
            category switch
            {
                VolumeCategory.Master => _data.MasterVolume,
                VolumeCategory.Music => _data.MusicVolume,
                VolumeCategory.Effects => _data.EffectsVolume,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };

        public void SetVolume(VolumeCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);

            switch (category)
            {
                case VolumeCategory.Master:
                    _data.MasterVolume = volume;
                    break;
                case VolumeCategory.Music:
                    _data.MusicVolume = volume;
                    break;
                case VolumeCategory.Effects:
                    _data.EffectsVolume = volume;
                    break;
            }

            VolumeChanged?.Invoke(category, volume);
        }

        public void ApplySavedLanguage()
        {
            var languageKey = LocalizationHelper.ContainsLanguageKey(Languages, _data.LanguageKey)
                ? _data.LanguageKey
                : LocalizationHelper.GetDefaultLanguageKey(Languages, Application.systemLanguage);

            SetLanguage(languageKey);
        }

        public void SetLanguage(string languageKey)
        {
            if (!LocalizationHelper.ContainsLanguageKey(Languages, languageKey))
            {
                Debug.LogWarning($"SettingsService:: Unknown language {languageKey}");
                return;
            }

            _data.LanguageKey = languageKey;
            _localizationService.SetLanguage(languageKey, LoadLanguageDictionary(languageKey));
        }

        private static Dictionary<string, string> LoadLanguageDictionary(string languageKey)
        {
            var textAsset = Resources.Load<TextAsset>(string.Format(LanguageDictionaryPathFormat, languageKey));
            return LocalizationParser.ParseToDictionary(textAsset.text);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Scripts.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private Dictionary<string, string> _dictionary = new();

        public event Action<string> LanguageChanged;
        public string LanguageKey { get; private set; }

        public static LocalizationService Instance;

        public void Awake()
        {
            Instance = this;
        }

        public void SetLanguage(string languageKey, Dictionary<string, string> dictionary)
        {
            LanguageKey = languageKey;
            _dictionary = dictionary;

            LanguageChanged?.Invoke(LanguageKey);
        }

        public bool ContainsKey(string key) =>
            _dictionary.ContainsKey(key);

        public string Get(string key) =>
            !_dictionary.ContainsKey(key) ? key.Trim() : _dictionary[key].Trim();

        public string Get(string key, params object[] args) =>
            string.Format(Get(key), args);
    }
}
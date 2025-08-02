using System;
using System.Collections.Generic;

namespace Scripts.Services.Localization
{
    public interface ILocalizationService
    {
        event Action<string> LanguageChanged;
        string LanguageKey { get; }
        void SetLanguage(string languageKey, Dictionary<string, string> dictionary);
        bool ContainsKey(string key);
        string Get(string key);
        string Get(string key, params object[] args);
    }
}
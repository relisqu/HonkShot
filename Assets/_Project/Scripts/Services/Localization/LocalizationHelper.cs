using System;
using System.Linq;
using UnityEngine;

namespace Scripts.Services.Localization
{
    public static class LocalizationHelper
    {
        public static bool ContainsLanguageKey(Language[] languages, string languageKey)
        {
            foreach (var language in languages)
                if (language.Key == languageKey)
                    return true;

            return false;
        }

        public static string GetDefaultLanguageKey(Language[] languages, SystemLanguage systemLanguage)
        {
            foreach (var language in languages)
                if (language.SystemLanguages.Contains(systemLanguage))
                    return language.Key;
            
            foreach (var language in languages)
                if (language.Default)
                    return language.Key;
            
            throw new Exception("Not found default language");
        }
    }
}
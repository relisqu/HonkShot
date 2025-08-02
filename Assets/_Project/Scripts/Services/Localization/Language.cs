using System;
using UnityEngine;

namespace Scripts.Services.Localization
{
    [Serializable]
    public class Language
    {
        public string Key;
        public string Name;
        public SystemLanguage[] SystemLanguages;
        public bool Default;

        public override string ToString() => 
            $"[{GetType().Name}] " +
            $"{nameof(Key)}: {Key}, " +
            $"{nameof(Name)}: {Name}, " +
            $"{nameof(SystemLanguages)}: {string.Join(",", SystemLanguages)}, " +
            $"{nameof(Default)}: {Default}";
    }
}
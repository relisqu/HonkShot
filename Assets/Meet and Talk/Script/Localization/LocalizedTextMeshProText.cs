using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace MeetAndTalk.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextMeshProText : MonoBehaviour
    {
        TMP_Text _textField;
        public List<LanguageGeneric<string>> localizations = new List<LanguageGeneric<string>>();

        private void OnValidate()
        {
            _textField = GetComponent<TMP_Text>();

            var enumValues = System.Enum.GetValues(typeof(LocalizationEnum)).Cast<LocalizationEnum>();
            foreach (var lang in enumValues)
            {
                if (!localizations.Any(l => l.languageEnum.Equals(lang)))
                {
                    localizations.Add(new LanguageGeneric<string> { languageEnum = lang, LanguageGenericType = "" });
                }
            }
        }

        private void Update()
        {
            LocalizationManager lm = Resources.Load("Languages") as LocalizationManager;
            if (_textField != null) _textField.text = localizations.Find(text => text.languageEnum == lm.SelectedLang()).LanguageGenericType;
        }
    }
}

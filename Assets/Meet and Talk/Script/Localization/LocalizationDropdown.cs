using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeetAndTalk.Localization
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class LocalizationDropdown : MonoBehaviour
    {
        TMPro.TMP_Dropdown dropdown;

        private void Awake()
        {
            RegenerateOptions();
            dropdown.onValueChanged.AddListener(value => ChangeLanguage(value));
        }

        public void OnValidate()
        {
            RegenerateOptions();
        }

        void RegenerateOptions()
        {
            dropdown = GetComponent<TMPro.TMP_Dropdown>();

            dropdown.options.Clear();
            dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(SystemLanguage.English.ToString()));
            
            LocalizationManager lm = Resources.Load("Languages") as LocalizationManager;
            for(int i = 0; i < lm.lang.Count; i++)
            {
                dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(lm.lang[i].ToString()));
            }
        }

        void ChangeLanguage(int value)
        {
            LocalizationManager lm = Resources.Load("Languages") as LocalizationManager;

            if (value == 0) { lm.selectedLang = SystemLanguage.English; }
            else
            {
                lm.selectedLang = lm.lang[value - 1];
            }
        }
    }
}

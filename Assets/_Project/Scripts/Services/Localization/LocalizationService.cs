using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Services.Localization
{
    public class LocalizationService : MonoBehaviour, ILocalizationService
    {
        private Dictionary<string, string> _dictionary = new();

        public event Action<string> LanguageChanged;
        public string LanguageKey { get; private set; }

        public static LocalizationService Instance;

        public void Awake()
        {
            Instance = this;
            SetLanguage("ru", null);
        }

        public void SetLanguage(string languageKey, Dictionary<string, string> dictionary)
        {
            LanguageKey = languageKey;
            _dictionary = LocalizationParser.ParseToDictionary(
                "Time.TimeFormat ~ hh\\:mm\\:ss\nTime.DateFormat ~ dd-MM-yyyy\nTime.DateTimeFormat ~ HH:mm:ss dd-MM-yyyy\nTime.DateTime1 ~ {0} д. {1} ч.\nTime.DateTime2 ~ {0} ч. {1} м.\nTime.DateTime3 ~ {0} м.\nTime.DateTime4 ~ {0} c.\nTime.TimeLeft ~ Осталось: {0}\nTime.LeftDays ~ {0} дней\nTime.LeftHours ~ {0} часов\nTime.LeftMinutes ~ {0} минут\nTime.LeftSeconds ~ {0} секунд\nTime.LeftLessHour ~ менее часа\nApp.Language ~ Русский\nitem_0_title ~ Cтальные накладки\nitem_1_title ~ Жёсткий пух\nitem_2_title ~ Запускатель\nitem_3_title ~ Каменная кожа\nitem_4_title ~ Увесистый пинок\nitem_5_title ~ Жиры\nitem_6_title ~ Белки\nitem_7_title ~ Фальшстарт\nitem_8_title ~ Легкие кости\nitem_9_title ~ Огненные сапоги\nitem_10_title ~ Выхлоп\nitem_11_title ~ Грязные лапы\nitem_12_title ~ Смертельное комбо\nitem_13_title ~ Ударная зарядка\nitem_14_title ~ Неприкасаемый\nitem_15_title ~ Запасные перья\nitem_16_title ~ Слепая ярость\nitem_17_title ~ Зеркало\nitem_18_title ~ Крит\nitem_19_title ~ Поглощение\nitem_20_title ~ Гвозди\nitem_21_title ~ Быстрое заживление\nitem_22_title ~ Стеклянная пушка\nitem_23_title ~ Жеский\nitem_24_title ~ Репей\n");

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
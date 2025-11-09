using System.Collections.Generic;
using Scripts.Services.Localization;
using UnityEngine;
using Zenject;

namespace Scripts.Managers
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
        }

        public override void Start()
        {
            base.Start();
            Container.Resolve<ILocalizationService>().SetLanguage("Ru", LoadLanguageDictionary("Ru"));
        }

        public static string GetLanguageDictionaryPath(string languageKey) =>
            $"Data/{languageKey}";

        public Dictionary<string, string> LoadLanguageDictionary(string languageKey)
        {
            var textAsset = Resources.Load<TextAsset>(GetLanguageDictionaryPath(languageKey));
            return LocalizationParser.ParseToDictionary(textAsset.text);
        }
    }
}
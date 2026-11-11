using Scripts.Services.Localization;
using Scripts.Services.Settings;
using UnityEngine;
using Zenject;

namespace Scripts.Managers
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        private const string LanguagesConfigPath = "Data/Configs/LanguagesConfig";

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
            Container.Bind<LanguagesConfigSO>()
                .FromInstance(Resources.Load<LanguagesConfigSO>(LanguagesConfigPath))
                .AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsService>().AsSingle().NonLazy();
        }

        public override void Start()
        {
            base.Start();
            Container.Resolve<SettingsService>().ApplySavedLanguage();
        }
    }
}

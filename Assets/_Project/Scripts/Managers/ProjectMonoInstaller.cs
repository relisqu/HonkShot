using Scripts.Services.Localization;
using Zenject;

namespace Scripts.Managers
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<LocalizationService>().AsSingle().NonLazy();
        }
    }
}
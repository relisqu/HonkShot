using UnityEngine;
using Zenject;

namespace Scripts.LevelSystem.LevelGeneration.Factories
{
    public class LevelObjectsFactory
    {
        DiContainer _container;

        [Inject]
        private void Inject(DiContainer container)
        {
            _container = container;
        }

        public LevelEnterPortal SpawnLevelEnterPortal(Transform parent)
        {
            var levelEnterPortalPrefab = Resources.Load<LevelEnterPortal>(Constants.LevelEnterPortalPrefabPath);
            return _container.InstantiatePrefab(levelEnterPortalPrefab, parent).GetComponent<LevelEnterPortal>();
        }

        public LevelExitPortal SpawnLevelExitPortal(Transform parent)
        {
            var levelExitPortalPrefab = Resources.Load<LevelExitPortal>(Constants.LevelExitPortalPrefabPath);
            return _container.InstantiatePrefab(levelExitPortalPrefab, parent).GetComponent<LevelExitPortal>();
        }
    }
}
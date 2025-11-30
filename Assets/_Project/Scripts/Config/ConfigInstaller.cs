using UnityEngine;
using Zenject;

namespace Scripts.Config
{
    public class ConfigInstaller : MonoInstaller
    {
        [Header("Config References")]
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private EnvironmentConfig _environmentConfig;
        
        [Header("Runtime References")]
        [SerializeField] private Scripts.PointSystem.PointReceiver _pointReceiver;
        
        public override void InstallBindings()
        {
            // Bind configs (ScriptableObjects are already instances, use FromInstance)
            if (_playerConfig != null)
            {
                Container.Bind<PlayerConfig>().FromInstance(_playerConfig).AsSingle();
            }
            
            if (_enemyConfig != null)
            {
                Container.Bind<EnemyConfig>().FromInstance(_enemyConfig).AsSingle();
            }
            
            if (_environmentConfig != null)
            {
                Container.Bind<EnvironmentConfig>().FromInstance(_environmentConfig).AsSingle();
            }
            
            // Bind PointReceiver
            if (_pointReceiver != null)
            {
                Container.Bind<Scripts.PointSystem.PointReceiver>().FromInstance(_pointReceiver).AsSingle();
            }
            else
            {
                // Try to find it in scene
                var pointReceiver = FindObjectOfType<Scripts.PointSystem.PointReceiver>();
                if (pointReceiver != null)
                {
                    Container.Bind<Scripts.PointSystem.PointReceiver>().FromInstance(pointReceiver).AsSingle();
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Config
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Config/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Default Settings (used if no ID match)")]
        public EnemyConfigData defaultSettings = new EnemyConfigData();
        
        [Header("Per-Enemy Settings (by ID)")]
        [Tooltip("Configure individual enemies by their ID")]
        public List<EnemyConfigData> enemySettings = new List<EnemyConfigData>();
        
        public EnemyConfigData GetSettingsForEnemy(int enemyId)
        {
            if (enemyId == 0)
            {
                return defaultSettings;
            }
            
            foreach (var settings in enemySettings)
            {
                if (settings.enemyId == enemyId)
                {
                    return settings;
                }
            }
            
            return defaultSettings;
        }
        
        public void AddOrUpdateEnemySettings(int enemyId, EnemyConfigData data)
        {
            for (int i = 0; i < enemySettings.Count; i++)
            {
                if (enemySettings[i].enemyId == enemyId)
                {
                    enemySettings[i] = data;
                    return;
                }
            }
            
            data.enemyId = enemyId;
            enemySettings.Add(data);
        }
    }
}

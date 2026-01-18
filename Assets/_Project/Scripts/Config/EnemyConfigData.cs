using System;
using System.Linq;
using Scripts.Enemies;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Config
{
    public enum EnemyType
    {
        Unknown,
        WalkingEnemy,
        ShooterEnemy,
        ManiacEnemy,
        TowardsPlayerMover,
        NoAIEnemy
    }

    [Serializable]
    public class EnemyConfigData
    {
        [Header("Enemy Identification")]
        [OnValueChanged("OnPrefabChanged")]
        [PreviewField(55, ObjectFieldAlignment.Left)]
        [HideLabel]
        [Title("Enemy Prefab")]
        public GameObject enemyPrefab;
        
        [ReadOnly]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [HideLabel]
        [Title("Detected Enemy Type")]
        private EnemyType _detectedEnemyType = EnemyType.Unknown;
        
        [PropertyOrder(0)]
        [ShowIf("@_detectedEnemyType != EnemyType.Unknown")]
        public int enemyId = 0;
        
        [PropertyOrder(0)]
        [ShowIf("@_detectedEnemyType != EnemyType.Unknown")]
        public string enemyName = "";
        
        [Header("Health Settings")]
        [PropertyOrder(1)]
        public float maxHealth = 100f;
        public float defaultHealth = 100f;

        [Header("Score Settings")]
        [PropertyOrder(1)]
        [Range(1, 10)]
        [Tooltip("Difficulty rating for score calculation (1 = easy, 10 = boss)")]
        public int difficultyRating = 1;
        
        [Header("Walking Enemy Settings")]
        [PropertyOrder(2)]
        [ShowIf("@_detectedEnemyType == EnemyType.WalkingEnemy || _detectedEnemyType == EnemyType.ManiacEnemy")]
        public float walkMoveSpeed = 2f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.WalkingEnemy || _detectedEnemyType == EnemyType.ManiacEnemy")]
        public float attackRange = 1.5f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.WalkingEnemy || _detectedEnemyType == EnemyType.ManiacEnemy")]
        public float attackInterval = 1f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.WalkingEnemy || _detectedEnemyType == EnemyType.ManiacEnemy")]
        public float attackWarningTime = 0.5f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.WalkingEnemy || _detectedEnemyType == EnemyType.ManiacEnemy")]
        public int damage = 1;
        
        [Header("Shooting Enemy Settings")]
        [PropertyOrder(3)]
        [ShowIf("@_detectedEnemyType == EnemyType.ShooterEnemy")]
        public float shootInterval = 2f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.ShooterEnemy")]
        public float bulletSpeed = 5f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.ShooterEnemy")]
        public float bulletDamage = 5f;
        
        [Header("Chase Settings")]
        [PropertyOrder(4)]
        [ShowIf("@_detectedEnemyType == EnemyType.ManiacEnemy || _detectedEnemyType == EnemyType.TowardsPlayerMover")]
        public float chaseSpeed = 3f;
        
        [ShowIf("@_detectedEnemyType == EnemyType.TowardsPlayerMover")]
        public float detectionRange = 5f;
        
        [Button("Auto-Detect Enemy Type")]
        [PropertyOrder(-2)]
        private void DetectEnemyType()
        {
            OnPrefabChanged();
        }
        
        private void OnPrefabChanged()
        {
            if (!enemyPrefab)
            {
                _detectedEnemyType = EnemyType.Unknown;
                return;
            }
            
            // Check for enemy types (order matters - check more specific first)
            if (enemyPrefab.GetComponent<ManiacEnemy>() != null)
            {
                _detectedEnemyType = EnemyType.ManiacEnemy;
            }
            else if (enemyPrefab.GetComponent<ShooterEnemy>() != null)
            {
                _detectedEnemyType = EnemyType.ShooterEnemy;
            }
            else if (enemyPrefab.GetComponent<WalkingEnemy>() != null)
            {
                _detectedEnemyType = EnemyType.WalkingEnemy;
            }
            else if (enemyPrefab.GetComponent<TowardsPlayerMover>() != null)
            {
                _detectedEnemyType = EnemyType.TowardsPlayerMover;
            }
            else if (enemyPrefab.GetComponent<NoAIEnemy>() != null)
            {
                _detectedEnemyType = EnemyType.NoAIEnemy;
            }
            else if (enemyPrefab.GetComponent<BaseEnemy>() != null)
            {
                // Has BaseEnemy but unknown type
                _detectedEnemyType = EnemyType.Unknown;
            }
            else
            {
                _detectedEnemyType = EnemyType.Unknown;
            }
            
            // Auto-load enemy ID if BaseEnemy exists
            var baseEnemy = enemyPrefab.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                enemyId = baseEnemy.EnemyId;
                enemyName = baseEnemy.EnemyName;
            }
        }
        
        public EnemyConfigData()
        {
            // Default values
        }
        
        public EnemyConfigData(int id)
        {
            enemyId = id;
        }
    }
}

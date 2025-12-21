using System.Linq;
using UnityEngine;
using Scripts.Health;
using Scripts.Player.Dash;
using Scripts.Enemies;
using Scripts.Enemies.Bullets;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelGeneration;
using Sirenix.OdinInspector;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Config
{
    public class GameConfigManager : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private EnvironmentConfig _environmentConfig;

        [Header("Prefab References (for applying to prefabs)")] [SerializeField]
        private GameObject _playerPrefab;

        [SerializeField] private GameObject[] _enemyPrefabs;

        [Header("Runtime References (optional - will find automatically if not set)")] [SerializeField]
        private HealthController _playerHealthController;

        [SerializeField] private PlayerAttackController _playerAttackController;
        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private ShieldController _playerShieldController;
        [SerializeField] private Scripts.PointSystem.PointReceiver _pointReceiver;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private LevelGenerator _levelGenerator;
        [SerializeField] private LevelTransitionManager _levelTransitionManager;

        [Header("Settings")] [SerializeField] private bool _applyToPrefabs = true;
        [SerializeField] private bool _applyToSceneObjects = true;
        [SerializeField] private bool _autoApplyOnStart = false;

        private bool _isApplied = false;

        [Button("Apply Configs")]
        public void ApplyConfigs()
        {
            Debug.Log("=== Starting Config Application ===");

            if (_playerConfig != null)
            {
                Debug.Log("Applying Player Config...");
                ApplyPlayerConfig();
            }
            else
            {
                Debug.LogWarning("Player Config is not assigned!");
            }

            if (_enemyConfig != null)
            {
                Debug.Log("Applying Enemy Config...");
                ApplyEnemyConfig();
            }
            else
            {
                Debug.LogWarning("Enemy Config is not assigned!");
            }

            if (_environmentConfig != null)
            {
                Debug.Log("Applying Environment Config...");
                ApplyEnvironmentConfig();
            }
            else
            {
                Debug.LogWarning("Environment Config is not assigned!");
            }

            _isApplied = true;
            Debug.Log("=== Config Application Complete ===");
        }

        private void ApplyPlayerConfig()
        {
            // Apply to prefab first
            if (_applyToPrefabs && _playerPrefab != null)
            {
                ApplyPlayerConfigToObject(_playerPrefab, "Prefab");
            }

            // Apply to scene objects
            if (_applyToSceneObjects)
            {
                // Find player in scene
                var playerHealth = _playerHealthController ?? FindObjectOfType<HealthController>();
                if (playerHealth != null && playerHealth.CompareTag("Player"))
                {
                    ApplyPlayerConfigToObject(playerHealth.gameObject, "Scene");
                }

                // Also apply to PointReceiver if it exists separately
                var pointReceiver = _pointReceiver ?? FindObjectOfType<Scripts.PointSystem.PointReceiver>();
                if (pointReceiver != null)
                {
                    SetSerializedProperty(pointReceiver, "_baseDamage", _playerConfig.baseDamage);
                    Debug.Log($"  - PointReceiver Base Damage: {_playerConfig.baseDamage}");
                }
            }
        }

        private void ApplyPlayerConfigToObject(GameObject targetObject, string source)
        {
            Debug.Log($"Applying player config to {source}: {targetObject.name}");

            // Apply health settings
            var healthController = targetObject.GetComponent<HealthController>();
            if (healthController != null)
            {
                SetSerializedProperty(healthController, "_maxHealth", _playerConfig.maxHealth);
                SetSerializedProperty(healthController, "_defaultHealth", _playerConfig.defaultHealth);
                Debug.Log($"  - Health: max={_playerConfig.maxHealth}, default={_playerConfig.defaultHealth}");
            }

            // Apply attack settings
            var attackController = targetObject.GetComponent<PlayerAttackController>();
            if (attackController != null)
            {
                SetSerializedProperty(attackController, "_dependsOnSpeed", _playerConfig.dependsOnSpeed);
                SetSerializedProperty(attackController, "_minBonusCoeffSpeed", _playerConfig.minBonusCoeffSpeed);
                SetSerializedProperty(attackController, "_maxBonusCoeffSpeed", _playerConfig.maxBonusCoeffSpeed);
                SetSerializedProperty(attackController, "_maxSpeedCoeff", _playerConfig.maxSpeedCoeff);
                Debug.Log(
                    $"  - Attack: dependsOnSpeed={_playerConfig.dependsOnSpeed}, minSpeed={_playerConfig.minBonusCoeffSpeed}, maxSpeed={_playerConfig.maxBonusCoeffSpeed}, speedCoeff={_playerConfig.maxSpeedCoeff}");
            }

            // Apply base damage to PointReceiver
            var pointReceiver = targetObject.GetComponent<Scripts.PointSystem.PointReceiver>();
            if (pointReceiver != null)
            {
                SetSerializedProperty(pointReceiver, "_baseDamage", _playerConfig.baseDamage);
                Debug.Log($"  - Base Damage: {_playerConfig.baseDamage}");
            }

            // Apply dash settings
            var dashController = targetObject.GetComponent<PlayerDashController>();
            if (dashController != null)
            {
                SetSerializedProperty(dashController, "_maxDashCount", _playerConfig.maxDashCount);
                SetSerializedProperty(dashController, "_canRegenDashes", _playerConfig.canRegenDashes);
                SetSerializedProperty(dashController, "_dashRegenerationRate", _playerConfig.dashRegenerationRate);
                Debug.Log(
                    $"  - Dash: maxCount={_playerConfig.maxDashCount}, canRegen={_playerConfig.canRegenDashes}, regenRate={_playerConfig.dashRegenerationRate}");
            }

            // Apply shield settings
            var shieldController = targetObject.GetComponent<ShieldController>();
            if (shieldController != null)
            {
                SetSerializedProperty(shieldController, "_maxShields", _playerConfig.maxShields);
                Debug.Log($"  - Shields: max={_playerConfig.maxShields}");
            }

            // Apply ball movement settings
            var playerBallMovement = targetObject.GetComponent<Scripts.Player.PlayerBallMovement>();
            if (playerBallMovement != null)
            {
                SetSerializedProperty(playerBallMovement, "_forceModifier", _playerConfig.forceModifier);
                SetSerializedProperty(playerBallMovement, "_maxForceMagnitude", _playerConfig.overallMaxForceMagnitude);
                SetSerializedProperty(playerBallMovement, "_maxDragForceMagnitude",
                    _playerConfig.maxForceMagnitudePerDrag);
                SetSerializedProperty(playerBallMovement, "_minBallForce", _playerConfig.minBallForce);
                SetSerializedProperty(playerBallMovement, "_additionalDragStartTime",
                    _playerConfig.additionalDragStartTime);
                SetSerializedProperty(playerBallMovement, "_additionalDragForce", _playerConfig.additionalDragForce);
                Debug.Log(
                    $"  - Ball Movement: forceModifier={_playerConfig.forceModifier}, maxForce={_playerConfig.overallMaxForceMagnitude}, maxDrag={_playerConfig.maxForceMagnitudePerDrag}");
            }

            // Apply fire system settings
            var gooseFireSystem = targetObject.GetComponent<Scripts.Player.GooseFireSystem>();
            if (gooseFireSystem != null)
            {
                SetSerializedProperty(gooseFireSystem, "_ultimateMultiplyCoefficient",
                    _playerConfig.ultimateMultiplyCoefficient);
                SetSerializedProperty(gooseFireSystem, "_ultimateDragCoefficient",
                    _playerConfig.ultimateDragCoefficient);
                SetSerializedProperty(gooseFireSystem, "_maxFire", _playerConfig.maxFire);
                SetSerializedProperty(gooseFireSystem, "_fireGainPerLaunch", _playerConfig.fireGainPerLaunch);
                SetSerializedProperty(gooseFireSystem, "_fireGainPerAcceleration",
                    _playerConfig.fireGainPerAcceleration);
                SetSerializedProperty(gooseFireSystem, "_fireLossPerDeceleration",
                    _playerConfig.fireLossPerDeceleration);
                SetSerializedProperty(gooseFireSystem, "_fireLossPerSecond", _playerConfig.fireLossPerSecond);
                SetSerializedProperty(gooseFireSystem, "_ultimateFireDrainPerSecond",
                    _playerConfig.ultimateFireDrainPerSecond);
                SetSerializedProperty(gooseFireSystem, "_honkFireDrainPerSecond", _playerConfig.honkFireDrainPerSecond);
                Debug.Log(
                    $"  - Fire System: maxFire={_playerConfig.maxFire}, gainPerLaunch={_playerConfig.fireGainPerLaunch}");
            }

            // Note: jumpForce and moveSpeed might not be used in this game
            // They're kept in config for potential future use
        }

        private void ApplyEnemyConfig()
        {
            if (_enemyConfig == null) return;

            // Apply to prefabs first
            if (_applyToPrefabs && _enemyPrefabs != null)
            {
                foreach (var enemyPrefab in _enemyPrefabs)
                {
                    if (enemyPrefab != null)
                    {
                        ApplyEnemyConfigToObject(enemyPrefab, "Prefab");
                    }
                }
            }

            // Apply to scene objects
            if (_applyToSceneObjects)
            {
                var baseEnemies = FindObjectsOfType<BaseEnemy>();
                foreach (var enemy in baseEnemies)
                {
                    ApplyEnemyConfigToObject(enemy.gameObject, "Scene");
                }
            }
        }

        private void ApplyEnemyConfigToObject(GameObject targetObject, string source)
        {
            var baseEnemy = targetObject.GetComponent<BaseEnemy>();
            if (baseEnemy == null)
            {
                Debug.LogWarning($"No BaseEnemy component found on {targetObject.name}");
                return;
            }

            int enemyId = baseEnemy.EnemyId;
            var settings = _enemyConfig.GetSettingsForEnemy(enemyId);

            if (settings == null) return;
            Debug.Log($"Applying enemy config to {source}: {targetObject.name} (ID: {enemyId})");

            // Apply health settings (all enemies have health)
            var enemyHealth = targetObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.HealthController != null)
            {
                var healthController = enemyHealth.HealthController;
                SetSerializedProperty(healthController, "_maxHealth", settings.maxHealth);
                SetSerializedProperty(healthController, "_defaultHealth", settings.defaultHealth);
                Debug.Log($"  - Health: max={settings.maxHealth}, default={settings.defaultHealth}");
            }

            // Apply walking enemy settings
            var walkingEnemy = targetObject.GetComponent<WalkingEnemy>();
            if (walkingEnemy != null)
            {
                SetSerializedProperty(walkingEnemy, "_moveSpeed", settings.walkMoveSpeed);
                SetSerializedProperty(walkingEnemy, "_attackRange", settings.attackRange);
                SetSerializedProperty(walkingEnemy, "_attackInterval", settings.attackInterval);
                SetSerializedProperty(walkingEnemy, "_attackWarningTime", settings.attackWarningTime);
                SetSerializedProperty(walkingEnemy, "_damage", settings.damage);
                Debug.Log($"  - Walking Enemy: speed={settings.walkMoveSpeed}, damage={settings.damage}");
            }

            // Apply maniac enemy settings
            var maniacEnemy = targetObject.GetComponent<ManiacEnemy>();
            if (maniacEnemy != null)
            {
                SetSerializedProperty(maniacEnemy, "_movementSpeed", settings.chaseSpeed);
                SetSerializedProperty(maniacEnemy, "_attackRange", settings.attackRange);
                SetSerializedProperty(maniacEnemy, "_attackInterval", settings.attackInterval);
                SetSerializedProperty(maniacEnemy, "_attackWarningTime", settings.attackWarningTime);
                SetSerializedProperty(maniacEnemy, "_damage", settings.damage);
                Debug.Log($"  - Maniac Enemy: speed={settings.chaseSpeed}, damage={settings.damage}");
            }

            // Apply shooting enemy settings
            var shooterEnemy = targetObject.GetComponent<ShooterEnemy>();
            if (shooterEnemy != null)
            {
                SetSerializedProperty(shooterEnemy, "_shootInterval", settings.shootInterval);
                Debug.Log($"  - Shooter Enemy: shootInterval={settings.shootInterval}");

                // Apply bullet settings to bullet prefab if ShootingModule exists
                var shootingModule = targetObject.GetComponentInChildren<ShootingModule>();
                if (shootingModule != null)
                {
                    // Try to get bullet prefab from ShootingModule
                    var bulletPrefabField = shootingModule.GetType().GetField("_bulletPrefab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (bulletPrefabField != null)
                    {
                        var bulletPrefab = bulletPrefabField.GetValue(shootingModule) as GameObject;
                        if (bulletPrefab != null)
                        {
                            var baseBullet = bulletPrefab.GetComponent<Scripts.Enemies.Bullets.BaseBullet>();
                            if (baseBullet != null)
                            {
                                SetSerializedProperty(baseBullet, "_speed", settings.bulletSpeed);
                                SetSerializedProperty(baseBullet, "_damage", settings.bulletDamage);
                                Debug.Log($"  - Bullet: speed={settings.bulletSpeed}, damage={settings.bulletDamage}");
                            }
                        }
                    }
                }
            }

            // Apply towards player mover settings
            var towardsPlayerMover = targetObject.GetComponent<TowardsPlayerMover>();
            if (towardsPlayerMover != null)
            {
                SetSerializedProperty(towardsPlayerMover, "_moveSpeed", settings.chaseSpeed);
                SetSerializedProperty(towardsPlayerMover, "_chaseRange", settings.detectionRange);
                Debug.Log($"  - Towards Player Mover: speed={settings.chaseSpeed}, range={settings.detectionRange}");
            }
        }

        private void ApplyEnvironmentConfig()
        {
            // Apply to level manager
            if (_levelTransitionManager != null)
            {
                SetSerializedProperty(_levelTransitionManager, "_itemScreenSpawnChance",
                    _environmentConfig.itemScreenSpawnChance);
                SetSerializedProperty(_levelTransitionManager, "_itemScreenPerRoomRate",
                    _environmentConfig.itemScreenPerRoomRate);
                Debug.Log(
                    $"  - LevelManager: spawnChance={_environmentConfig.itemScreenSpawnChance}, rate={_environmentConfig.itemScreenPerRoomRate}");
            }
            else if (_applyToSceneObjects)
            {
                var _levelTransitionManager = FindObjectOfType<LevelTransitionManager>();
                if (_levelTransitionManager != null)
                {
                    SetSerializedProperty(_levelTransitionManager, "_itemScreenSpawnChance",
                        _environmentConfig.itemScreenSpawnChance);
                    SetSerializedProperty(_levelTransitionManager, "_itemScreenPerRoomRate",
                        _environmentConfig.itemScreenPerRoomRate);
                    Debug.Log(
                        $"  - LevelManager (found): spawnChance={_environmentConfig.itemScreenSpawnChance}, rate={_environmentConfig.itemScreenPerRoomRate}");
                }
            }

            // Apply to level generator
            if (_levelGenerator != null)
            {
                SetSerializedProperty(_levelGenerator, "_roomCount", _environmentConfig.roomCount);
                SetSerializedProperty(_levelGenerator, "_roomHeight", _environmentConfig.roomHeight);
                Debug.Log(
                    $"  - LevelGenerator: roomCount={_environmentConfig.roomCount}, roomHeight={_environmentConfig.roomHeight}");
            }
            else if (_applyToSceneObjects)
            {
                var levelGenerator = FindObjectOfType<LevelGenerator>();
                if (levelGenerator != null)
                {
                    SetSerializedProperty(levelGenerator, "_roomCount", _environmentConfig.roomCount);
                    SetSerializedProperty(levelGenerator, "_roomHeight", _environmentConfig.roomHeight);
                    Debug.Log(
                        $"  - LevelGenerator (found): roomCount={_environmentConfig.roomCount}, roomHeight={_environmentConfig.roomHeight}");
                }
            }
        }

        private void SetSerializedProperty(UnityEngine.Object target, string propertyName, object value)
        {
            if (target == null)
            {
                Debug.LogWarning($"Target is null for property '{propertyName}'");
                return;
            }

#if UNITY_EDITOR
            // Use SerializedObject in editor (more reliable)
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                switch (value)
                {
                    case float floatValue:
                        property.floatValue = floatValue;
                        break;
                    case int intValue:
                        property.intValue = intValue;
                        break;
                    case bool boolValue:
                        property.boolValue = boolValue;
                        break;
                    default:
                        Debug.LogWarning($"Unsupported value type: {value.GetType()}");
                        return;
                }

                serializedObject.ApplyModifiedProperties();

                // Mark prefab as dirty if it's a prefab
                if (PrefabUtility.IsPartOfPrefabAsset(target))
                {
                    EditorUtility.SetDirty(target);
                }

                return;
            }
#endif

            // Fallback to reflection (works in both editor and runtime)
            var field = target.GetType().GetField(propertyName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                try
                {
                    field.SetValue(target, value);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to set field '{propertyName}' on {target.GetType().Name}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Field/Property '{propertyName}' not found on {target.GetType().Name}. Available fields: {string.Join(", ", target.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Select(f => f.Name))}");
            }
        }

        [Button("Save Configs to Prefabs")]
        public void SaveConfigsToPrefabs()
        {
#if UNITY_EDITOR
            if (!_applyToPrefabs)
            {
                Debug.LogWarning("Apply to Prefabs is disabled. Enable it first.");
                return;
            }

            Debug.Log("=== Saving Configs to Prefabs ===");

            if (_playerConfig != null && _playerPrefab != null)
            {
                ApplyPlayerConfigToObject(_playerPrefab, "Prefab");
                PrefabUtility.SaveAsPrefabAsset(_playerPrefab,
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_playerPrefab));
                Debug.Log($"Saved player config to prefab: {_playerPrefab.name}");
            }

            if (_enemyConfig != null && _enemyPrefabs != null)
            {
                foreach (var enemyPrefab in _enemyPrefabs)
                {
                    if (enemyPrefab != null)
                    {
                        ApplyEnemyConfigToObject(enemyPrefab, "Prefab");
                        PrefabUtility.SaveAsPrefabAsset(enemyPrefab,
                            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(enemyPrefab));
                        Debug.Log($"Saved enemy config to prefab: {enemyPrefab.name}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("=== Prefab Save Complete ===");
#else
            Debug.LogWarning("Save function is only available in Editor mode!");
#endif
        }

        [Button("Load Configs from Prefabs")]
        public void LoadConfigsFromPrefabs()
        {
#if UNITY_EDITOR
            if (_playerConfig == null)
            {
                Debug.LogWarning("Player Config is not assigned!");
                return;
            }

            Debug.Log("=== Loading Configs from Prefabs ===");

            if (_playerPrefab != null)
            {
                LoadPlayerConfigFromObject(_playerPrefab);
                EditorUtility.SetDirty(_playerConfig);
                Debug.Log($"Loaded player config from prefab: {_playerPrefab.name}");
            }
            else
            {
                Debug.LogWarning("Player Prefab is not assigned!");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("=== Config Load Complete ===");
#else
            Debug.LogWarning("Load function is only available in Editor mode!");
#endif
        }

        private void LoadPlayerConfigFromObject(GameObject sourceObject)
        {
            Debug.Log($"Loading player config from: {sourceObject.name}");

            // Load health settings
            var healthController = sourceObject.GetComponent<HealthController>();
            if (healthController != null)
            {
                _playerConfig.maxHealth = GetSerializedProperty<float>(healthController, "_maxHealth");
                _playerConfig.defaultHealth = GetSerializedProperty<float>(healthController, "_defaultHealth");
                Debug.Log($"  - Health: max={_playerConfig.maxHealth}, default={_playerConfig.defaultHealth}");
            }

            // Load attack settings
            var attackController = sourceObject.GetComponent<PlayerAttackController>();
            if (attackController != null)
            {
                _playerConfig.dependsOnSpeed = GetSerializedProperty<bool>(attackController, "_dependsOnSpeed");
                _playerConfig.minBonusCoeffSpeed =
                    GetSerializedProperty<float>(attackController, "_minBonusCoeffSpeed");
                _playerConfig.maxBonusCoeffSpeed =
                    GetSerializedProperty<float>(attackController, "_maxBonusCoeffSpeed");
                _playerConfig.maxSpeedCoeff = GetSerializedProperty<float>(attackController, "_maxSpeedCoeff");
                Debug.Log(
                    $"  - Attack: dependsOnSpeed={_playerConfig.dependsOnSpeed}, minSpeed={_playerConfig.minBonusCoeffSpeed}, maxSpeed={_playerConfig.maxBonusCoeffSpeed}, speedCoeff={_playerConfig.maxSpeedCoeff}");
            }

            // Load base damage from PointReceiver
            var pointReceiver = sourceObject.GetComponent<Scripts.PointSystem.PointReceiver>();
            if (pointReceiver != null)
            {
                _playerConfig.baseDamage = GetSerializedProperty<float>(pointReceiver, "_baseDamage");
                Debug.Log($"  - Base Damage: {_playerConfig.baseDamage}");
            }

            // Load dash settings
            var dashController = sourceObject.GetComponent<PlayerDashController>();
            if (dashController != null)
            {
                _playerConfig.maxDashCount = GetSerializedProperty<int>(dashController, "_maxDashCount");
                _playerConfig.canRegenDashes = GetSerializedProperty<bool>(dashController, "_canRegenDashes");
                _playerConfig.dashRegenerationRate =
                    GetSerializedProperty<float>(dashController, "_dashRegenerationRate");
                Debug.Log(
                    $"  - Dash: maxCount={_playerConfig.maxDashCount}, canRegen={_playerConfig.canRegenDashes}, regenRate={_playerConfig.dashRegenerationRate}");
            }

            // Load shield settings
            var shieldController = sourceObject.GetComponent<ShieldController>();
            if (shieldController != null)
            {
                _playerConfig.maxShields = GetSerializedProperty<int>(shieldController, "_maxShields");
                Debug.Log($"  - Shields: max={_playerConfig.maxShields}");
            }

            // Load ball movement settings
            var playerBallMovement = sourceObject.GetComponent<Scripts.Player.PlayerBallMovement>();
            if (playerBallMovement != null)
            {
                _playerConfig.forceModifier = GetSerializedProperty<float>(playerBallMovement, "_forceModifier");
                _playerConfig.overallMaxForceMagnitude =
                    GetSerializedProperty<float>(playerBallMovement, "_maxForceMagnitude");
                _playerConfig.maxForceMagnitudePerDrag =
                    GetSerializedProperty<float>(playerBallMovement, "_maxDragForceMagnitude");
                _playerConfig.minBallForce = GetSerializedProperty<float>(playerBallMovement, "_minBallForce");
                _playerConfig.additionalDragStartTime =
                    GetSerializedProperty<float>(playerBallMovement, "_additionalDragStartTime");
                _playerConfig.additionalDragForce =
                    GetSerializedProperty<float>(playerBallMovement, "_additionalDragForce");
                Debug.Log(
                    $"  - Ball Movement: forceModifier={_playerConfig.forceModifier}, maxForce={_playerConfig.overallMaxForceMagnitude}, maxDrag={_playerConfig.maxForceMagnitudePerDrag}");
            }

            // Load fire system settings
            var gooseFireSystem = sourceObject.GetComponent<Scripts.Player.GooseFireSystem>();
            if (gooseFireSystem != null)
            {
                _playerConfig.ultimateMultiplyCoefficient =
                    GetSerializedProperty<float>(gooseFireSystem, "_ultimateMultiplyCoefficient");
                _playerConfig.ultimateDragCoefficient =
                    GetSerializedProperty<float>(gooseFireSystem, "_ultimateDragCoefficient");
                _playerConfig.maxFire = GetSerializedProperty<float>(gooseFireSystem, "_maxFire");
                _playerConfig.fireGainPerLaunch = GetSerializedProperty<float>(gooseFireSystem, "_fireGainPerLaunch");
                _playerConfig.fireGainPerAcceleration =
                    GetSerializedProperty<float>(gooseFireSystem, "_fireGainPerAcceleration");
                _playerConfig.fireLossPerDeceleration =
                    GetSerializedProperty<float>(gooseFireSystem, "_fireLossPerDeceleration");
                _playerConfig.fireLossPerSecond = GetSerializedProperty<float>(gooseFireSystem, "_fireLossPerSecond");
                _playerConfig.ultimateFireDrainPerSecond =
                    GetSerializedProperty<float>(gooseFireSystem, "_ultimateFireDrainPerSecond");
                _playerConfig.honkFireDrainPerSecond =
                    GetSerializedProperty<float>(gooseFireSystem, "_honkFireDrainPerSecond");
                Debug.Log(
                    $"  - Fire System: maxFire={_playerConfig.maxFire}, gainPerLaunch={_playerConfig.fireGainPerLaunch}");
            }
        }

        private T GetSerializedProperty<T>(UnityEngine.Object target, string propertyName)
        {
            if (target == null)
            {
                Debug.LogWarning($"Target is null for property '{propertyName}'");
                return default(T);
            }

#if UNITY_EDITOR
            // Use SerializedObject in editor (more reliable)
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                if (typeof(T) == typeof(float))
                {
                    return (T)(object)property.floatValue;
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)property.intValue;
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)property.boolValue;
                }
                else
                {
                    Debug.LogWarning($"Unsupported type for property '{propertyName}': {typeof(T)}");
                    return default(T);
                }
            }
#endif

            // Fallback to reflection (works in both editor and runtime)
            var field = target.GetType().GetField(propertyName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                try
                {
                    var value = field.GetValue(target);
                    if (value is T)
                    {
                        return (T)value;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Field '{propertyName}' type mismatch. Expected {typeof(T)}, got {value?.GetType()}");
                        return default(T);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to get field '{propertyName}' from {target.GetType().Name}: {e.Message}");
                    return default(T);
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Field/Property '{propertyName}' not found on {target.GetType().Name}. Available fields: {string.Join(", ", target.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Select(f => f.Name))}");
                return default(T);
            }
        }

        void Start()
        {
            // Auto-apply configs on start if enabled
            if (_autoApplyOnStart && !_isApplied)
            {
                ApplyConfigs();
            }
        }
    }
}
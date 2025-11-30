using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Config
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Health Settings")] public float maxHealth = 100f;
        public float defaultHealth = 100f;

        [Header("Attack Settings")] public float baseDamage = 10f;
        public bool dependsOnSpeed = false;
        public float minBonusCoeffSpeed = 5f;
        public float maxBonusCoeffSpeed = 10f;
        public float maxSpeedCoeff = 2f;

        [Header("Dash Settings")] public int maxDashCount = 3;
        public bool canRegenDashes = true;
        public float dashRegenerationRate = 0.4f;

        [Header("Shield Settings")] public int maxShields = 3;

        [Header("Movement Settings")] public float moveSpeed = 5f;

        [Header("Ball Movement Settings")] public float forceModifier = 1f;

        [FormerlySerializedAs("maxForceMagnitude")]
        public float overallMaxForceMagnitude = 10f;

        [FormerlySerializedAs("maxDragForceMagnitude")]
        public float maxForceMagnitudePerDrag = 20f;

        public float minBallForce = 0f;

        [Tooltip("The goose starts slowing down faster if it is rolling for a long time")]
        public float additionalDragStartTime = 2000f;

        public float additionalDragForce = 1.01f;

        [Header("Fire System Settings")] public float ultimateMultiplyCoefficient = 5f;
        public float ultimateDragCoefficient = 0f;
        public float maxFire = 100f;
        public float fireGainPerLaunch = 20f;
        public float fireGainPerAcceleration = 5f;
        public float fireLossPerDeceleration = 10f;
        public float fireLossPerSecond = 5f;
        public float ultimateFireDrainPerSecond = 20f;
        public float honkFireDrainPerSecond = 30f;
    }
}
using UnityEngine;

namespace Scripts.Config
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "Config/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [Header("Item Spawn Settings")]
        [Range(0f, 1f)]
        public float itemScreenSpawnChance = 0.5f;
        
        [Min(1)]
        [Tooltip("Once per how many rooms item room will try spawn")]
        public int itemScreenPerRoomRate = 2;
        
        [Header("Level Generation Settings")]
        [Min(1)]
        public int roomCount = 5;
        
        [Header("Room Settings")]
        public float roomHeight = 10f;
    }
}

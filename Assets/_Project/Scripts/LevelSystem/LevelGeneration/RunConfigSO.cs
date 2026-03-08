using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [CreateAssetMenu(fileName = "NewRunConfig", menuName = "Game/Run Config")]
    public class RunConfigSO : ScriptableObject
    {
        [SerializeField] private List<FloorConfigSO> _floors;

        public List<FloorConfigSO> Floors => _floors;

        public FloorConfigSO GetFloorConfig(int index)
        {
            if (index < 0 || index >= _floors.Count)
                return null;
            return _floors[index];
        }
    }
}

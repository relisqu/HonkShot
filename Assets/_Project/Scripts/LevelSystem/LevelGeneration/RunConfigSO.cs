using System.Collections.Generic;
using Sirenix.OdinInspector;
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

        // Handled by RoomPrefabRefresher in the Editor assembly. Nothing subscribes at runtime.
        public static System.Action<RunConfigSO> RefreshRoomsRequested;

        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        [InfoBox("Rebuilds walls and corner decorations inside every room prefab of every floor in this run.")]
        public void RefreshAllRooms()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[RunConfigSO] RefreshAllRooms only works in Edit Mode — prefab assets cannot be written during Play Mode.");
                return;
            }

            if (RefreshRoomsRequested == null)
            {
                Debug.LogError("[RunConfigSO] RoomPrefabRefresher is not loaded — is Assets/_Project/Scripts/Editor/RoomPrefabRefresher.cs compiling?");
                return;
            }

            RefreshRoomsRequested.Invoke(this);
        }
    }
}

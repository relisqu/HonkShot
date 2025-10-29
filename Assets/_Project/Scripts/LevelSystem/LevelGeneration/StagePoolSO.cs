using System;
using System.Collections.Generic;
using Scripts.LevelSystem.LevelObjects;
using Scripts.LevelSystem.TechnicalScripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [CreateAssetMenu(fileName = "NewStagePool", menuName = "Game/Stage Pool")]
    public class StagePoolSO : ScriptableObject
    {
        public List<RoomModel> RoomModels;

        [Button]
        public void RefreshRooms()
        {
            foreach (var roomModel in RoomModels)
            {
                roomModel.RoomPrefab.GetComponent<WallGeneration>().CopySpriteShapeSpline();
                roomModel.RoomPrefab.GetComponent<CornerGenerator>().Generate();
            }
        }
    }
}
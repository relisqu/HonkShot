using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [CreateAssetMenu(fileName = "NewStagePool", menuName = "Game/Stage Pool")]
    public class StagePoolSO : ScriptableObject
    {
        public List<RoomModel> RoomModels;
    }
}
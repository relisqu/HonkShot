using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [CreateAssetMenu(fileName = "FloorDecorations", menuName = "Game/Floor Decorations")]
    public class FloorDecorationsSO : ScriptableObject
    {
        [Serializable]
        public class PropDefinition
        {
            [Tooltip("Label for Inspector only.")]
            public string id;

            [Tooltip("Sprites for this prop type. One is picked randomly.")]
            public Sprite[] sprites;

            [Tooltip("If true, flip X so the prop appears to 'lean' toward the map center.")]
            public bool faceLevelCenter = false;

            [Tooltip("Random uniform scale multiplier AFTER parallax scale.")]
            public Vector2 randomScaleRange = new Vector2(0.9f, 1.1f);

            [Tooltip("Max random XY jitter offset (world units) applied after placement.")]
            public Vector2 positionJitterRange = new Vector2(0.2f, 0.2f);

            [Tooltip("If false, this prop will NOT receive wind animation (WindFactor forced 0).")]
            public bool affectedByWind = true;
        }

        [SerializeField] private List<PropDefinition> _propDefinitions = new List<PropDefinition>();

        public List<PropDefinition> PropDefinitions => _propDefinitions;
    }
}

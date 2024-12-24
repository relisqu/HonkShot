using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Audio
{
    [CreateAssetMenu(fileName = "SoundTable", menuName = "Audio/New Sound Table")]
    public class SoundTableSO : SerializedScriptableObject
    {
        public Dictionary<string, SoundEffectSO> Sounds;
    }
}
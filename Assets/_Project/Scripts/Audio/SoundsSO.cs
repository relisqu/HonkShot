using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Audio
{
    [CreateAssetMenu(fileName = "SoundEffect", menuName = "Audio/New Sound Effect")]
    public class SoundsSO : SerializedScriptableObject
    {
        public Dictionary<string, SoundEffectSO> Sounds;
    }
}
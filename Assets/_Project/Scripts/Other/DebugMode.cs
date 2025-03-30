using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Other
{
    public class DebugMode : MonoBehaviour
    {
        [FormerlySerializedAs("NeedGenerateLevels")] public bool GeneratingLevels;

        public static DebugMode Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}
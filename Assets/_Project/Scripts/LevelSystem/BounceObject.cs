using System;
using Scripts.Audio;
using Scripts.Player;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField] private float _bounciness;
        public float Bounciness => _bounciness;

    }
}
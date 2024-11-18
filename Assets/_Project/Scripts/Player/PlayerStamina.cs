using System;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private float _defaultStamina;
        private float _currentStamina;

        public float CurrentStamina => _currentStamina;

        private void Start()
        {
            _currentStamina = _defaultStamina;
        }
    }
}
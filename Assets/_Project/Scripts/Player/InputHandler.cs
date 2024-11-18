using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveActionReference;

        private void Start()
        {
            _moveActionReference.action.Enable();
        }

        public Vector2 GetMovementInput()
        {
            return _moveActionReference.action.phase == InputActionPhase.Performed
                ? _moveActionReference.action.ReadValue<Vector2>()
                : Vector2.zero;
        }
    }
}
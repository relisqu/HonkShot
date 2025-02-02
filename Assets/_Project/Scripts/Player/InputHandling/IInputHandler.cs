using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player.InputHandling
{
    public interface IInputHandler
    {
        event Action OnDragStarted;
        event Action OnVisualDragStarted;
        event Action<Vector2> OnDragFinished;


        InputActionReference MovementValueActionInterface { get; }
        InputActionReference PressActionInterface { get; }
        bool IsDragging { get; }
        void Enable();
        void Disable();
        Vector2 GetCurrentDrag();
        public void UpdateCurrentMovementDelta();
    }
}
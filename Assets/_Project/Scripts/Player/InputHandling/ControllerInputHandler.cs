using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player.InputHandling
{
    public class ControllerInputHandler : IInputHandler
    {
        private InputActionReference _movementValueActionInterface;
        private InputActionReference _pressActionInterface;
        private Transform _playerTransform;

        private Vector2 _dragStartPos;
        private Vector2 _dragCurPos;
        private bool _isDragging = false;

        public event Action OnDragStarted;
        public event Action<Vector2> OnDragFinished;
        public InputActionReference MovementValueActionInterface => _movementValueActionInterface;
        public InputActionReference PressActionInterface => _pressActionInterface;

        public bool IsDragging => _isDragging;

        public ControllerInputHandler(InputActionReference movementValueActionInterface,
            InputActionReference pressActionInterface)
        {
            _movementValueActionInterface = movementValueActionInterface;
            _pressActionInterface = pressActionInterface;
        }

        public void Enable()
        {
            _movementValueActionInterface.action.Enable();
            _pressActionInterface.action.started += OnDragStart;
            _pressActionInterface.action.canceled += OnDragRelease;
        }

        public void Disable()
        {
            _movementValueActionInterface.action.Disable();
            _pressActionInterface.action.started -= OnDragStart;
            _pressActionInterface.action.canceled -= OnDragRelease;
        }

        public void UpdateCurrentMovementDelta()
        {
            _dragCurPos += _movementValueActionInterface.action.ReadValue<Vector2>().normalized * Time.deltaTime;
        }

        private void OnDragStart(InputAction.CallbackContext context)
        {
            _isDragging = true;
            OnDragStarted?.Invoke();
            _dragStartPos = new Vector2();
            _dragCurPos = _dragStartPos;
        }

        private void OnDragRelease(InputAction.CallbackContext context)
        {
            _isDragging = false;
            OnDragFinished?.Invoke(GetCurrentDrag());
        }

        public Vector2 GetCurrentDrag()
        {
            return _dragCurPos;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public event Action OnVisualDragStarted;
        public event Action<Vector2> OnDragFinished;
        public event Action OnCancelInput;

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
            _pressActionInterface.action.Enable();
            _movementValueActionInterface.action.Enable();
            _movementValueActionInterface.action.started += OnVisualDragStart;
            _pressActionInterface.action.started += OnDragStart;
            _pressActionInterface.action.canceled += OnDragRelease;
        }

        public void Disable()
        {
            _pressActionInterface.action.Disable();
            _movementValueActionInterface.action.Disable();
            _movementValueActionInterface.action.started -= OnVisualDragStart;
            _pressActionInterface.action.started -= OnDragStart;
            _pressActionInterface.action.canceled -= OnDragRelease;
        }

        public void UpdateCurrentMovementDelta()
        {
            _dragCurPos = _movementValueActionInterface.action.ReadValue<Vector2>();
        }

        public void CancelInput()
        {
            OnCancelInput?.Invoke();
            _isDragging = false;
        }

        private void OnVisualDragStart(InputAction.CallbackContext context)
        {
            Debug.Log("Started visual drag");
            OnVisualDragStarted?.Invoke();
        }

        private void OnDragStart(InputAction.CallbackContext context)
        {
            Debug.Log("Started drag: " + _isDragging);
            _isDragging = true;
            OnDragStarted?.Invoke();
            _dragStartPos = new Vector2();
            _dragCurPos = _dragStartPos;
        }

        private void OnDragRelease(InputAction.CallbackContext context)
        {
            Debug.Log("Released drag: " + _isDragging);
            _isDragging = false;
            OnDragFinished?.Invoke(GetCurrentDrag());
        }

        public Vector2 GetCurrentDrag()
        {
            return _dragCurPos;
        }
    }
}
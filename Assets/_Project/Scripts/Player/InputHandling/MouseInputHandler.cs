using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player.InputHandling
{
    public class MouseInputHandler : IInputHandler
    {
        private InputActionReference _mousePositionAction;
        private InputActionReference _pressAction;
        private Vector2 _dragStartPos;
        private Vector2 _dragCurPos;
        private UnityEngine.Camera _camera;
        private bool _isDragging = false;
        private Transform _playerTransform;

        public event Action OnDragStarted;
        public event Action<Vector2> OnDragFinished;
        public InputActionReference MovementValueActionInterface => _mousePositionAction;
        public InputActionReference PressActionInterface => _pressAction;

        public bool IsDragging => _isDragging;

        public MouseInputHandler(InputActionReference mousePositionAction, InputActionReference pressAction,
            Transform playerTransform, UnityEngine.Camera camera)
        {
            _camera = camera;
            _mousePositionAction = mousePositionAction;
            _pressAction = pressAction;
            _playerTransform = playerTransform;
        }

        public void Enable()
        {
            _pressAction.action.Enable();
            _mousePositionAction.action.Enable();
            _pressAction.action.started += OnDragStart;
            _pressAction.action.canceled += OnDragRelease;
        }

        public void Disable()
        {
            _pressAction.action.Disable();
            _mousePositionAction.action.Disable();
            _pressAction.action.started -= OnDragStart;
            _pressAction.action.canceled -= OnDragRelease;
        }

        public void UpdateCurrentMovementDelta()
        {
            var currentPoint = (Vector2)_camera.ScreenToWorldPoint(_mousePositionAction.action.ReadValue<Vector2>());
            _dragCurPos = currentPoint - _dragStartPos;
        }

        private void OnDragStart(InputAction.CallbackContext context)
        {
            _dragCurPos = Vector2.zero;
            _isDragging = true;
            OnDragStarted?.Invoke();
            _dragStartPos = (Vector2)_playerTransform.position;
            var currentPoint = (Vector2)_camera.ScreenToWorldPoint(_mousePositionAction.action.ReadValue<Vector2>());
            _dragCurPos = currentPoint - _dragStartPos;
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
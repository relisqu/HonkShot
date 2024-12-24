using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveActionReference;
        [SerializeField] private InputActionReference _mouseActionReference;
        [SerializeField] private Transform _playerTransform;


        private Vector2 _dragCurPos;
        private Vector2 _dragStartPos;
        private bool _isDragging = false;

        public Action OnDragStarted;
        public Action<Vector2> OnDragFinished;
        public bool IsDragging => _isDragging;

        private void Start()
        {
            _moveActionReference.action.Enable();
            _mouseActionReference.action.Enable();
        }

        private void Update()
        {
            if (_isDragging)
            {
                // Continuously update the drag position
                _dragCurPos = _mouseActionReference.action.ReadValue<Vector2>();
            }
        }

        public Vector2 GetMovementInput()
        {
            return _moveActionReference.action.phase == InputActionPhase.Performed
                ? _moveActionReference.action.ReadValue<Vector2>()
                : Vector2.zero;
        }

        private Vector2 _mouseInput = Vector2.zero;

        private void OnEnable()
        {
            _mouseActionReference.action.started += OnDragStart;
            _mouseActionReference.action.canceled += OnDragRelease;
        }

        private void OnDisable()
        {
            _mouseActionReference.action.Disable();

            _mouseActionReference.action.started -= OnDragStart;
            _mouseActionReference.action.canceled -= OnDragRelease;
            
        }
        
        private void OnDragStart(InputAction.CallbackContext context)
        {
            OnDragStarted?.Invoke();
            _isDragging = true;
            _dragStartPos = Camera.main.WorldToScreenPoint(_playerTransform.position);
            _dragCurPos = _mouseActionReference.action.ReadValue<Vector2>();
        }

        private void OnDragRelease(InputAction.CallbackContext context)
        {
            OnDragFinished?.Invoke(GetCurrentDrag());
            _isDragging = false;
        }

        
        public Vector2 GetCurrentDrag()
        {
            var dragDirection = _dragCurPos - _dragStartPos;
            return dragDirection;
        }

        public float GetCurrentDragMagnitude()
        {
            return GetCurrentDrag().magnitude;
        }
    }
}
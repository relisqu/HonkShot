using System;
using Scripts.Items.StatSystems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player.InputHandling
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _pressActionReference;
        [SerializeField] private InputActionReference _movementActionReference;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private PlayerInput _playerInput;

        private IInputHandler _currentInputHandler;
        private bool _inputEnabled => _inputModifierSystem?.Calculate(true) ?? false;

        private BoolStatModifierSystem _inputModifierSystem = new BoolStatModifierSystem();
        public event Action OnDragStarted;
        public event Action<Vector2> OnDragFinished;
        public bool IsDragging => _currentInputHandler.IsDragging;

        public void SetInputEnabled(InputLayer inputLayer, bool value)
        {
            Debug.Log($"InputHandler:: Set layer {inputLayer} to {value}");
            if (_inputModifierSystem.HasModifier((int)inputLayer))
            {
                _inputModifierSystem.UpdateModifier((int)inputLayer, value);
            }
            else
            {
                _inputModifierSystem.AddModifier(new BoolStatModifier((int)inputLayer, BoolModType.And, value, 1));
            }
        }

        public bool IsInputEnabled => _inputEnabled;

        private void Awake()
        {
            _pressActionReference.action.Enable();
        }

        private void Start()
        {
            _inputModifierSystem = new BoolStatModifierSystem();
            ChangeControlScheme(_playerInput.currentControlScheme.ToLower());
            SetInputEnabled(InputLayer.InputSystem, true);
        }

        private void Update()
        {
            if (IsInputEnabled)
                _currentInputHandler?.UpdateCurrentMovementDelta();
        }

        public void InputHandler_DragStarted()
        {
            if (IsInputEnabled)
                OnDragStarted?.Invoke();
        }

        public void InputHandler_DragFinished(Vector2 dragFinished)
        {
            if (IsInputEnabled)
                OnDragFinished?.Invoke(dragFinished);
        }

        private void ChangeControlScheme(string newControlScheme)
        {
            if (_currentInputHandler != null)
            {
                _currentInputHandler.OnDragStarted -= InputHandler_DragStarted;
                _currentInputHandler.OnDragFinished -= InputHandler_DragFinished;
                _currentInputHandler.Disable();
            }

            switch (newControlScheme)
            {
                case "controller":
                    _currentInputHandler = new ControllerInputHandler(_movementActionReference, _pressActionReference);
                    break;
                case "keyboard":
                    _currentInputHandler = new MouseInputHandler(_movementActionReference, _pressActionReference,
                        _playerTransform, _camera);
                    break;
                default:
                    Debug.LogWarning($"Unknown control scheme: {newControlScheme}");
                    break;
            }

            if (_currentInputHandler != null)
            {
                _currentInputHandler.OnDragStarted += InputHandler_DragStarted;
                _currentInputHandler.OnDragFinished += InputHandler_DragFinished;
                _currentInputHandler?.Enable();
            }
        }

        private void PlayerInput_ControlsChanged(PlayerInput playerInput)
        {
            ChangeControlScheme(playerInput.currentControlScheme.ToLower());
        }

        private void OnEnable()
        {
            _playerInput.onControlsChanged += PlayerInput_ControlsChanged;
        }

        private void OnDisable()
        {
            if (_currentInputHandler != null)
            {
                _currentInputHandler.OnDragStarted -= InputHandler_DragStarted;
                _currentInputHandler.OnDragFinished -= InputHandler_DragFinished;
                _currentInputHandler.Disable();
            }

            _playerInput.onControlsChanged -= PlayerInput_ControlsChanged;
        }


        public Vector2 GetCurrentDrag()
        {
            if (!_inputEnabled) return Vector2.zero;
            return _currentInputHandler?.GetCurrentDrag() ?? Vector2.zero;
        }
    }
}
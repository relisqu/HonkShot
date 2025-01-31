using System;
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

        public event Action OnDragStarted;
        public event Action<Vector2> OnDragFinished;
        public bool IsDragging => _currentInputHandler.IsDragging;

        private void Awake()
        {
            _pressActionReference.action.Enable();
        }

        private void Start()
        {
            ChangeControlScheme(_playerInput.currentControlScheme.ToLower());
        }

        private void Update()
        {
            _currentInputHandler?.UpdateCurrentMovementDelta();
        }

        private void ChangeControlScheme(string newControlScheme)
        {
            if (_currentInputHandler != null)
            {
                _currentInputHandler.OnDragStarted -= OnDragStarted;
                _currentInputHandler.OnDragFinished -= OnDragFinished;
                _currentInputHandler.Disable();
            }
            switch (newControlScheme)
            {
                case "controller":
                    _currentInputHandler = new ControllerInputHandler(_movementActionReference, _pressActionReference);
                    break;
                case "keyboard":
                    _currentInputHandler = new MouseInputHandler(_movementActionReference, _pressActionReference, _playerTransform, _camera);
                    break;
                default:
                    Debug.LogWarning($"Unknown control scheme: {newControlScheme}");
                    break;
            }
            
            Debug.Log(newControlScheme);
            if (_currentInputHandler != null)
            {
                _currentInputHandler.OnDragStarted += OnDragStarted;
                _currentInputHandler.OnDragFinished += OnDragFinished;
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
                _currentInputHandler.OnDragStarted -= OnDragStarted;
                _currentInputHandler.OnDragFinished -= OnDragFinished;
                _currentInputHandler.Disable();
            }
            _playerInput.onControlsChanged -= PlayerInput_ControlsChanged;
        }


        public Vector2 GetCurrentDrag()
        {
            return _currentInputHandler?.GetCurrentDrag() ?? Vector2.zero;
        }
    }
}
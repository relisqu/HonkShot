using System;
using Scripts.Audio;
using Scripts.Camera;
using Scripts.LevelSystem;
using Scripts.Player.Dash;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private InputHandler _inputHandler;

        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private GooseFireSystem _gooseFireSystem;

        public Action DragStarted;
        public Action<Vector2> DragFinished;

        private bool _isDragging;
        private float _prevSpeed;

        private void Awake()
        {
            if (_gooseFireSystem == null)
                _gooseFireSystem = GetComponent<GooseFireSystem>();
        }

        private void OnEnable()
        {
            _inputHandler.OnDragStarted += InputHandler_OnDragStarted;
            _inputHandler.OnDragFinished += InputHandler_OnDragFinished;
        }

        private void InputHandler_OnDragFinished(Vector2 obj)
        {
            if (_isDragging)
            {
                DragFinished?.Invoke(obj);
                _isDragging = false;
            }
        }

        private void OnDisable()
        {
            _inputHandler.OnDragStarted -= InputHandler_OnDragStarted;
            _inputHandler.OnDragFinished -= InputHandler_OnDragFinished;
        }

        private void InputHandler_OnDragStarted()
        {
            if (_playerDashController.CanDash)
            {
                _isDragging = true;
                _playerStatus.SetPlayerState(PlayerState.Swapping);
                DragStarted?.Invoke();
            }
        }
        private void FixedUpdate()
        {
            float currentSpeed = _rigidbody2D.velocity.magnitude;
            if (_gooseFireSystem)
            {
                if (currentSpeed > _prevSpeed + 0.01f)
                    _gooseFireSystem.OnAcceleration();
                else if (currentSpeed < _prevSpeed - 0.01f)
                    _gooseFireSystem.OnDeceleration();
            }
            _prevSpeed = currentSpeed;
            switch (_playerStatus.PlayerState)
            {
                case PlayerState.Ball:
                    
                    break;
                case PlayerState.Swapping:
                    break;
                default:
                    break;
            }
        }

        public Vector2 GetVelocity()
        {
            return _rigidbody2D.velocity;
        }


        public Rigidbody2D GetRigidbody()
        {
            return _rigidbody2D;
        }

    }
}
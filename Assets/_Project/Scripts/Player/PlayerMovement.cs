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
        public Action InputCancelled;

        /// <summary>Raised when the player tried to aim a dash with no dash charges left.</summary>
        public Action DashFailed;

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
            _inputHandler.OnInputCancelled += InputHandler_InputCancelled;
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
            _inputHandler.OnInputCancelled -= InputHandler_InputCancelled;
            _inputHandler.OnDragFinished -= InputHandler_OnDragFinished;
        }

        private void InputHandler_InputCancelled()
        {
            // Invoke first: listeners reset the scale of whatever CurrentGameObject is right now.
            InputCancelled?.Invoke();

            // Only revert when this drag actually moved the player into Swapping. A press that never
            // became an aim (no dash charge) must not transform the player.
            if (_isDragging)
                _playerStatus.ResetSwapState();

            _isDragging = false;
        }

        private void InputHandler_OnDragStarted()
        {
            if (_playerDashController.CanDash)
            {
                _isDragging = true;
                _playerStatus.SetPlayerState(PlayerState.Swapping);
                DragStarted?.Invoke();
                return;
            }

            DashFailed?.Invoke();
        }

        private void FixedUpdate()
        {
            float currentSpeed = _rigidbody2D.linearVelocity.magnitude;
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

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
        public Transform GetTransform()
        {
            return transform;
        }
        public Vector2 GetVelocity()
        {
            return _rigidbody2D.linearVelocity;
        }


        public Rigidbody2D GetRigidbody()
        {
            return _rigidbody2D;
        }

    }
}
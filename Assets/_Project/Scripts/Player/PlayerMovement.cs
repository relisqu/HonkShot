using System;
using Scripts.Audio;
using Scripts.Camera;
using Scripts.LevelSystem;
using Scripts.Player.Stamina;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private float _defaultSpeed;
        [SerializeField] private float _linearDrag = 0.05f;

        private void OnEnable()
        {
            _inputHandler.OnDragStarted += InputHandler_OnDragStarted;
        }

        private void OnDisable()
        {
            _inputHandler.OnDragStarted -= InputHandler_OnDragStarted;
        }

        private void InputHandler_OnDragStarted()
        {
            if (_staminaManager.TrySpendStamina(20f))
            {
                _playerStatus.SetPlayerState(PlayerState.Swapping);
            }
            else
            {
                CameraShakeHandler.Instance.ShakeCamera(0.55f, 10f);
            }
        }

        private void FixedUpdate()
        {
            switch (_playerStatus.PlayerState)
            {
                case PlayerState.Shooter:
                    _rigidbody2D.velocity *= _linearDrag;
                    Move();
                    break;
                case PlayerState.Ball:
                    break;
                case PlayerState.Swapping:
                    _rigidbody2D.velocity = Vector2.zero;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void Move()
        {
            var movement = _inputHandler.GetMovementInput();
            _rigidbody2D.AddForce(movement * (_defaultSpeed * Time.unscaledDeltaTime), ForceMode2D.Impulse);
            _rigidbody2D.velocity *= _linearDrag;
        }

        public float GetSpeed()
        {
            return _rigidbody2D.velocity.magnitude;
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
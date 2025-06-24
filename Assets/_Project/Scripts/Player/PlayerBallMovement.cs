using System;
using Scripts.Audio;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelObjects;
using Scripts.Player.Dash;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class PlayerBallMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private GooseFireSystem _gooseFireSystem;

        [Header("Input Handling Parameters")] [Space] [SerializeField]
        private float _forceModifier;

        [FormerlySerializedAs("_maxForce")] [SerializeField]
        private float _maxForceMagnitude;

        [SerializeField] private float _maxDragForceMagnitude = 20f;

        [SerializeField] private float _minBallForce;

        [Header("Ball Movement Parameters")] [Space] [SerializeField]
        private float _additionalDragStartTime = 2000f;

        [SerializeField] private float _additionalDragForce = 1.01f;
        public float MaxForceMagnitude => _maxForceMagnitude;

        private float _throwStartTime;
        private float _prevSpeed;

        private float _defaultDragValue;
        public float CurrentSpeed => _rigidbody2D.velocity.magnitude;

        private void Awake()
        {
            if (_gooseFireSystem == null)
                _gooseFireSystem = GetComponent<GooseFireSystem>();
        }

        private void Start()
        {
            _defaultDragValue = _rigidbody2D.drag;
        }

        private void Update()
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

            if (_playerStatus.PlayerState == PlayerState.Ball && Time.time - _throwStartTime > _additionalDragStartTime)
            {
                var additionalDragForce = Mathf.Clamp01(_additionalDragForce * Time.deltaTime);
                _rigidbody2D.drag += additionalDragForce * additionalDragForce;
            }
            else
            {
                _rigidbody2D.drag = _defaultDragValue;
            }
        }

        private void OnEnable()
        {
            _inputHandler.OnDragFinished += InputHandler_OnDragFinished;
        }

        private void OnDisable()
        {
            _inputHandler.OnDragFinished -= InputHandler_OnDragFinished;
        }

        private void InputHandler_OnDragFinished(Vector2 force)
        {
            if (_playerStatus.PlayerState == PlayerState.Swapping)
            {
                ThrowGoose(force);
                _playerDashController.DecreaseDashCount();
            }
        }

        public void ThrowGoose(Vector2 dragForce)
        {
            ThrowRigidbody(_rigidbody2D, dragForce);
            _playerStatus.SetPlayerState(PlayerState.Ball);
            _throwStartTime = Time.time;
            if (_gooseFireSystem != null)
                _gooseFireSystem.OnLaunchOrAcceleration();
        }

        public float GetDragVelocityMagnitude(Rigidbody2D rigidbody, Vector2 dragForce)
        {
            var forceMagnitude = Mathf.Min(dragForce.magnitude, _maxForceMagnitude) * _forceModifier;
            var currentVelocity = rigidbody.velocity.magnitude;
            if (currentVelocity + forceMagnitude > _maxDragForceMagnitude)
            {
                forceMagnitude = Mathf.Max(0, _maxDragForceMagnitude - currentVelocity);
            }

            return currentVelocity + forceMagnitude;
        }

        public void ThrowRigidbody(Rigidbody2D rigidbody, Vector2 dragForce)
        {
            var currentVelocity = GetDragVelocityMagnitude(rigidbody, dragForce);
            rigidbody.velocity = -currentVelocity * dragForce.normalized;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_playerStatus.PlayerState != PlayerState.Ball) return;

            if (other.gameObject.TryGetComponent(out LevelSolidObject _))
            {
                var force = _rigidbody2D.velocity.magnitude / _forceModifier;
                force = Mathf.Clamp(force, 0, 1);

                AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "gooseCollision", force);
            }
        }

        public void StopMovement()
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }
}
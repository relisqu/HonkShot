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

        [Header("Input Handling Parameters")] [Space] [SerializeField]
        private float _forceModifier;

        [FormerlySerializedAs("_maxForce")] [SerializeField]
        private float _maxForceMagnitude;

        [SerializeField] private float _minBallForce;

        [Header("Ball Movement Parameters")] [Space] [SerializeField]
        private float _additionalDragStartTime = 2000f;

        [SerializeField] private float _additionalDragForce = 1.01f;
        public float MaxForceMagnitude => _maxForceMagnitude;

        private float _throwStartTime;

        private float _defaultDragValue;
        public float CurrentSpeed => _rigidbody2D.velocity.magnitude;

        private void Start()
        {
            _defaultDragValue = _rigidbody2D.drag;
        }

        private void Update()
        {
            if (_playerStatus.PlayerState == PlayerState.Ball && Time.time - _throwStartTime > _additionalDragStartTime)
            {
                var additionalDragForce = Mathf.Clamp01(_additionalDragForce * Time.deltaTime);
                Debug.Log($"START ADDITIONAL DRAG {additionalDragForce * additionalDragForce}");
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
        }

        public void ThrowRigidbody(Rigidbody2D rigidbody, Vector2 dragForce)
        {
            var forceMagnitude = Mathf.Min(dragForce.magnitude, _maxForceMagnitude);
            rigidbody.AddForce(dragForce.normalized * (-forceMagnitude * _forceModifier), ForceMode2D.Impulse);
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
    }
}
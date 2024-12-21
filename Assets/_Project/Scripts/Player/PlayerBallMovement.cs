using System;
using Scripts.LevelObjects;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerBallMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private float _defaultSpeed;
        [SerializeField] private float _forceModifier;
        [SerializeField] private float _maxForce;
        [SerializeField] private float _minBallForce;

        public void Update()
        {
            if (_playerStatus.PlayerState == PlayerState.Ball && _rigidbody2D.velocity.magnitude < _minBallForce)
            {
                FinishBallMode();
            }
        }

        private void FinishBallMode()
        {
            _rigidbody2D.velocity = Vector2.zero;
            _playerStatus.SetPlayerState(PlayerState.Shooter);
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
            ThrowGoose(force);
        }

        public void ThrowGoose(Vector2 dragForce)
        {
            var force = dragForce.magnitude / _forceModifier;
            force = Mathf.Clamp(force, 0, _maxForce);
            _rigidbody2D.AddForce(-dragForce.normalized * force, ForceMode2D.Impulse);
            _playerStatus.SetPlayerState(PlayerState.Ball);
        }

        private Vector2 preCollisionVelocity;
        private void FixedUpdate()
        {
            // Capture the Rigidbody's velocity before the collision happens
            preCollisionVelocity = _rigidbody2D.velocity;
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_playerStatus.PlayerState != PlayerState.Ball) return;

            if (other.gameObject.TryGetComponent(out BounceObject bounceObject))
            {
                if (bounceObject.Bounciness <= 0) return;

                Vector2 normal = other.contacts[0].normal;
                Vector2 incomingVelocity = preCollisionVelocity;
                Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);
                _rigidbody2D.velocity = reflectedVelocity * bounceObject.Bounciness;
            }
        }
        
    }
}
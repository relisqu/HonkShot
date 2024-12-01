using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private float _forceModifier;
        [SerializeField] private float _defaultSpeed;
        [FormerlySerializedAs("_defaultForce")] [SerializeField] private float _maxForce;


        private void Update()
        {
            if (_playerStatus.PlayerState == PlayerState.Shooter)
            {
                Move();
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
            Debug.Log("AAA");
            ThrowGoose(force);
        }


        public void Move()
        {
            var movement = _inputHandler.GetMovementInput();
            _rigidbody2D.AddForce(movement * (_defaultSpeed * Time.deltaTime), ForceMode2D.Impulse);
        }

        private void ThrowGoose(Vector2 dragForce)
        {
            var force = dragForce.magnitude /_forceModifier;
            force = Mathf.Clamp(force, 0, _maxForce);
            _rigidbody2D.AddForce(-dragForce.normalized * force, ForceMode2D.Impulse);
            //_inputHandler.GetCurrentInputPosition();
        }

        private Vector2 CalculateThrowVelocity()
        {
            return Vector2.zero;
        }
    }
}
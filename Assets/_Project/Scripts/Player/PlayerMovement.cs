using System;
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
        [SerializeField] private float _defaultSpeed;
        [SerializeField] private float _linearDrag = 0.05f;


        private void Update()
        {
            if (_playerStatus.PlayerState == PlayerState.Shooter)
            {
                Move();
            }
        }

        private void FixedUpdate()
        {
            if (_playerStatus.PlayerState == PlayerState.Shooter)
            {
                _rigidbody2D.velocity *= _linearDrag;
            }
        }

        public void Move()
        {
            var movement = _inputHandler.GetMovementInput();
            _rigidbody2D.AddForce(movement * (_defaultSpeed * Time.deltaTime), ForceMode2D.Impulse);
            _rigidbody2D.velocity *= _linearDrag;
        }
    }
}
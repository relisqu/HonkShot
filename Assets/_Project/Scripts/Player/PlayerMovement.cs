using System;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private float _defaultSpeed;


        private void Update()
        {
            if (_playerStatus.PlayerState == PlayerState.Shooter)
            {
                Move();
            }
        }

        public void Move()
        {
            var movement = _inputHandler.GetMovementInput();
            _rigidbody2D.AddForce(movement * (_defaultSpeed * Time.deltaTime), ForceMode2D.Impulse);
        }
    }
}
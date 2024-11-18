using System;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        private PlayerState _playerState;

        public PlayerState PlayerState => _playerState;

        public void SetDefaultState()
        {
            _playerState = PlayerState.Shooter;
        }

        public void SetPlayerState(PlayerState playerState)
        {
            _playerState = playerState;
        }

        private void Start()
        {
            SetDefaultState();
        }
    }
}
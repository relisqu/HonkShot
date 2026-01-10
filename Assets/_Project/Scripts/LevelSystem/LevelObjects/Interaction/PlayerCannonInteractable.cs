using System.Numerics;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Player;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Scripts.Player.InputHandling;
using UnityEngine;
using Zenject;

namespace Scripts.LevelSystem.LevelObjects.Interaction
{
    public class PlayerCannonInteractable : CannonInteractable
    {
        private PlayerStatus _playerStatus;

        [Inject] private InputHandler _inputHandler;
        [SerializeField] private PlayerHealth _playerHealth;
        [Inject] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private FireParticleSystem _playerFireParticleSystem;

        protected override void Awake()
        {
            base.Awake();
            _playerStatus = GetComponent<PlayerStatus>();
        }

        public override void OnEnterCannon()
        {
            // Pause input for player
            if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, false);
            _playerHealth.HealthController.SetInvincible((int)InvincibilityEnum.Cannon, true);
            _playerFireParticleSystem.ClearParticleSystems();
            //  _playerStatus.HidePlayer();
            if (_rb)
            {
                _storedVelocity = _rb.linearVelocity;
                _rb.linearVelocity = Vector2.zero;
                _rb.isKinematic = true;
            }

            if (_collider)
                _collider.enabled = false;

            gameObject.transform.localScale = Vector3.zero;
        }

        public override void OnExitCannon(Vector2 shootDirection, float shootForce)
        {
            // Resume input for player
            _playerHealth.HealthController.SetInvincible((int)InvincibilityEnum.Cannon, false);
            _playerFireParticleSystem.ResumeParticleSystems();
            if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, true);
            if (_rb)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = shootDirection.normalized * shootForce;
            }

            if (_collider)
                _collider.enabled = true;

            gameObject.transform.localScale = Vector3.one;
        }
    }
}
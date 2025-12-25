using System.Numerics;
using Scripts.Player;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Scripts.Player.InputHandling;
using Zenject;

namespace Scripts.LevelSystem.LevelObjects.Interaction
{
    public class PlayerCannonInteractable : CannonInteractable
    {
        private PlayerStatus _playerStatus;

        [Inject] private InputHandler _inputHandler;
        [Inject] private PlayerBallMovement _playerBallMovement;

        protected override void Awake()
        {
            base.Awake();
            _playerStatus = GetComponent<PlayerStatus>();
        }

        public override void OnEnterCannon()
        {
            // Pause input for player
            if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, false);
            //  _playerStatus.HidePlayer();
            if (_rb != null)
            {
                _storedVelocity = _rb.linearVelocity;
                _rb.linearVelocity = Vector2.zero;
                _rb.isKinematic = true;
            }

            if (_collider != null)
                _collider.enabled = false;

            _playerBallMovement.HideTrail();
            gameObject.transform.localScale = Vector3.zero;
            
        }

        public override void OnExitCannon(Vector2 shootDirection, float shootForce)
        {
            // Resume input for player
            if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, true);
            if (_rb)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = shootDirection.normalized * shootForce;
            }

            if (_collider != null)
                _collider.enabled = true;

            _playerBallMovement.ResetTrail();
            gameObject.transform.localScale = Vector3.one;
        }
    }
}
using System.Numerics;
using Scripts.Player;
using Vector2 = UnityEngine.Vector2;
using Scripts.Player.InputHandling;

namespace Scripts.LevelSystem.LevelObjects.Interaction
{
    public class PlayerCannonInteractable : CannonInteractable
    {
        private PlayerStatus _playerStatus;

        protected override void Awake()
        {
            base.Awake();
            _playerStatus = GetComponent<PlayerStatus>();
        }

        public override void OnEnterCannon()
        {
            // Pause input for player
            if (InputHandler.Instance) InputHandler.Instance.SetInputEnabled(false);
            //  _playerStatus.HidePlayer();
            if (_rb != null)
            {
                _storedVelocity = _rb.linearVelocity;
                _rb.linearVelocity = Vector2.zero;
                _rb.isKinematic = true;
            }

            if (_collider != null)
                _collider.enabled = false;
            
            gameObject.transform.localScale*=0.01f;
        }

        public override void OnExitCannon(Vector2 shootDirection, float shootForce)
        {
            // Resume input for player
            if (InputHandler.Instance) InputHandler.Instance.SetInputEnabled(true);
            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = shootDirection.normalized * shootForce;
            }

            if (_collider != null)
                _collider.enabled = true;
            gameObject.transform.localScale*=100f;
        }
    }
}
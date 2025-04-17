using System.Numerics;
using Scripts.Player;
using Vector2 = UnityEngine.Vector2;

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
          //  _playerStatus.HidePlayer();
            if (_rb != null)
            {
                _storedVelocity = _rb.velocity;
                _rb.velocity = Vector2.zero;
                _rb.isKinematic = true;
            }

            if (_collider != null)
                _collider.enabled = false;
        }

        public override void OnExitCannon(Vector2 shootDirection, float shootForce)
        {

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.velocity = shootDirection.normalized * shootForce;
            }

            if (_collider != null)
                _collider.enabled = true;
           // _playerStatus.RevealPlayer();
        }
    }
}
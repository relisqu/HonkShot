using System;
using Scripts.Audio;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelObjects;
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
        [SerializeField] private float _forceModifier;

        [FormerlySerializedAs("_maxForce")] [SerializeField]
        private float _maxForceMagnitude;

        [SerializeField] private float _minBallForce;
        public float MaxForceMagnitude => _maxForceMagnitude;

        

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
                ThrowGoose(force);
        }

        public void ThrowGoose(Vector2 dragForce)
        {
            ThrowRigidbody(_rigidbody2D, dragForce);
            _playerStatus.SetPlayerState(PlayerState.Ball);
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
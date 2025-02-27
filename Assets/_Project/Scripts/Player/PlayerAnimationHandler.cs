using System;
using DG.Tweening;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Scripts.Player
{
    public class PlayerAnimationHandler : MonoBehaviour
    {
        private static readonly int SwapToBall = Animator.StringToHash("SwapToBall");
        private static readonly int SwapToShooter = Animator.StringToHash("SwapToShooter");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int DragStart = Animator.StringToHash("DragStart");


        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private SpriteRenderer _shooterSprite;
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private ParticleSystem _playerSweatParticleSystem;

        [FormerlySerializedAs("MinSize")] [SerializeField]
        private float _minSize;

        [FormerlySerializedAs("ShakeForce")] [SerializeField]
        private float _shakeForce;


        private float _lastHorizontalVelocity = 0f;

        private void OnEnable()
        {
            _playerStatus.AnimationStateMachine().SetAnimator(_playerAnimator);
            _playerStatus.OnSwapToBall += PlayerStatus_SwapToBall;
            _inputHandler.OnDragStarted += InputHandler_DragStarted;
            _inputHandler.OnDragFinished += InputHandler_DragFinished;
        }

        private void Update()
        {
            switch (_playerStatus.PlayerState)
            {
                case PlayerState.Ball:
                    break;
                case PlayerState.Swapping:
                    SetDragAnimation();
                    break;
            }
        }


        private void AlignShooterSpriteToMovement()
        {
            float currentVelocityX = _playerMovement.GetVelocity().x;
            if (!(Mathf.Abs(currentVelocityX) > 0f) ||
                Mathf.Approximately(Mathf.Sign(currentVelocityX), Mathf.Sign(_lastHorizontalVelocity))) return;

            var currentScale = currentVelocityX < 0 ? 1f : -1f;

            // _shooterGameObject.transform.localScale =
            //     new Vector3(currentScale, 1f, 1f);
            _lastHorizontalVelocity = currentVelocityX;
        }

        private void OnDisable()
        {
            _playerStatus.OnSwapToBall -= PlayerStatus_SwapToBall;
            _inputHandler.OnDragStarted -= InputHandler_DragStarted;
            _inputHandler.OnDragFinished -= InputHandler_DragFinished;
        }


        private void PlayerStatus_SwapToShooter()
        {
            PlaySwapToShooterAnimation();
        }

        private void PlayerStatus_SwapToBall()
        {
            PlaySwapToBallAnimation();
        }


        private Tweener _dragShakeTweener;

        public void SetDragAnimation()
        {
            Debug.Log(_playerStatus.CurrentGameObject.transform.localScale + " " +
                      _playerStatus.CurrentGameObject.name);
            if (_playerStatus.PlayerState == PlayerState.Swapping)
            {
                Debug.Log(_playerStatus.PlayerState);

                var forceScale = _inputHandler.GetCurrentDrag().magnitude / _playerBallMovement.MaxForceMagnitude;
                float currentYScale = Mathf.Lerp(1f, 0.8f, forceScale);
                float currentVelocityX = _inputHandler.GetCurrentDrag().x;
                var currentScale = currentVelocityX < 0 ? 1f : -1f;
                _playerStatus.CurrentGameObject.transform.localScale = new Vector3(currentScale, currentYScale, 1f);
                Debug.Log(_playerStatus.CurrentGameObject.transform.localScale + " " +
                          _playerStatus.CurrentGameObject.name);
            }
        }

        private void EmitSweatParticles()
        {
            var dragMagnitude = _inputHandler.GetCurrentDrag().magnitude;
            var forceMagnitude = Mathf.Min(dragMagnitude, _playerBallMovement.MaxForceMagnitude);
            var forceScale = forceMagnitude / _playerBallMovement.MaxForceMagnitude;

            var randomValue = Random.Range(0.4f, forceScale * 3f);
            _playerSweatParticleSystem.Emit((int)randomValue);
        }

        public void PlaySwapToBallAnimation()
        {
            _playerAnimator.SetTrigger(SwapToBall);
        }

        public void PlaySwapToShooterAnimation()
        {
            _playerAnimator.SetTrigger(SwapToShooter);
        }

        private void InputHandler_DragStarted()
        {
            _playerAnimator.SetTrigger(DragStart);
        }


        private void InputHandler_DragFinished(Vector2 _)
        {
            float currentVelocityX = _playerMovement.GetVelocity().x;

            var currentScale = currentVelocityX < 0 ? 1f : -1f;

            _playerStatus.CurrentGameObject.transform.localScale =
                new Vector3(currentScale, 1f, 1f);
            _lastHorizontalVelocity = currentVelocityX;
        }
    }
}
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Scripts.Player
{
    public class PlayerAnimationHandler : MonoBehaviour
    {
        private static readonly int SwapToBall = Animator.StringToHash("SwapToBall");
        private static readonly int SwapToShooter = Animator.StringToHash("SwapToShooter");
        private static readonly int Speed = Animator.StringToHash("Speed");

        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private SpriteRenderer _shooterSprite;
        [SerializeField] private GameObject _shooterGameObject;
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private InputHandler _inputHandler;

        [FormerlySerializedAs("MinSize")] [SerializeField]
        private float _minSize;

        [FormerlySerializedAs("ShakeForce")] [SerializeField]
        private float _shakeForce;


        private float _lastHorizontalVelocity = 0f;

        private void OnEnable()
        {
            _playerStatus.OnSwapToBall += PlayerStatus_SwapToBall;
            _playerStatus.OnSwapToShooter += PlayerStatus_SwapToShooter;
            _inputHandler.OnDragStarted += InputHandler_DragStarted;
            _inputHandler.OnDragFinished += InputHandler_DragFinished;
        }

        private void Update()
        {
            switch (_playerStatus.PlayerState)
            {
                case PlayerState.Shooter:
                    AlignShooterSpriteToMovement();
                    _playerAnimator.SetFloat(Speed, _playerMovement.GetSpeed());
                    break;

                case PlayerState.Ball:
                    break;
                case PlayerState.Swapping:
                    SetDragAnimation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void AlignShooterSpriteToMovement()
        {
            float currentVelocityX = _playerMovement.GetVelocity().x;
            if (!(Mathf.Abs(currentVelocityX) > 0.1f) ||
                Mathf.Approximately(Mathf.Sign(currentVelocityX), Mathf.Sign(_lastHorizontalVelocity))) return;

            var currentScale = currentVelocityX < 0 ? 1f : -1f;

            _shooterGameObject.transform.localScale =
                new Vector3(currentScale, 1f, 1f);
            _lastHorizontalVelocity = currentVelocityX;
        }

        private void OnDisable()
        {
            _playerStatus.OnSwapToBall -= PlayerStatus_SwapToBall;
            _playerStatus.OnSwapToShooter -= PlayerStatus_SwapToShooter;
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
            if (_playerStatus.PlayerState == PlayerState.Swapping)
            {
                var dragForce = _inputHandler.GetCurrentDragMagnitude() / 50000f;
                if (_dragShakeTweener == null)
                {
                    _dragShakeTweener = _shooterGameObject.transform.DOShakePosition(0.1f, dragForce * _shakeForce)
                        .OnComplete(
                            () => { _dragShakeTweener = null; });
                    _dragShakeTweener.Play();
                }


                var scale = Mathf.Clamp(8 / Mathf.Sqrt(_inputHandler.GetCurrentDragMagnitude()), 0.6f, 1f);

                float currentVelocityX = _inputHandler.GetCurrentDrag().x;

                var currentScale = currentVelocityX < 0 ? 1f : -1f;

                _shooterGameObject.transform.localScale =
                    new Vector3(-currentScale, scale, 1f);
            }
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
        }

        private void InputHandler_DragFinished(Vector2 _)
        {
            float currentVelocityX = _playerMovement.GetVelocity().x;

            var currentScale = currentVelocityX < 0 ? 1f : -1f;

            _shooterGameObject.transform.localScale =
                new Vector3(currentScale, 1f, 1f);
            _lastHorizontalVelocity = currentVelocityX;
        }
    }
}
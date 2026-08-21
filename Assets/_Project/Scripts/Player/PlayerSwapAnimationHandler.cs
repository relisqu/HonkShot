
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Scripts.Camera;
using Scripts.Player.InputHandling;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

namespace Scripts.Player
{
    public class PlayerSwapAnimationHandler : MonoBehaviour
    {
        private static readonly int SwapToBall = Animator.StringToHash("SwapToBall");
        private static readonly int SwapToShooter = Animator.StringToHash("SwapToShooter");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int DragStart = Animator.StringToHash("DragStart");

        [Header("References")] [SerializeField]
        private PlayerStatus _playerStatus;

        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private SpriteRenderer _shooterSprite;
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private ParticleSystem _playerSweatParticleSystem;

        [Header("Settings")] [SerializeField] private float _shakeForce;

        private float _lastHorizontalVelocity = 0f;

        private Tweener _dragShakeTweener;
        private TweenerCore<float, float, FloatOptions> _slowdownTween;

        private Coroutine _sweatEmitRoutine;
        private bool _isEmittingSweat = false;

        private void Start()
        {
            _playerStatus.AnimationStateMachine().SetAnimator(_playerAnimator);
        }

        private void OnEnable()
        {
            _playerStatus.OnSwapToBall += PlayerStatus_SwapToBall;
            _playerMovement.DragStarted += PlayerMovement_DragStarted;
            _playerMovement.DragFinished += PlayerMovement_DragFinished;
            _playerMovement.InputCancelled += PlayerMovement_InputCancelled;
        }

        private void OnDisable()
        {
            _playerStatus.OnSwapToBall -= PlayerStatus_SwapToBall;
            _playerMovement.DragStarted -= PlayerMovement_DragStarted;
            _playerMovement.DragFinished -= PlayerMovement_DragFinished;
            _playerMovement.InputCancelled -= PlayerMovement_InputCancelled;
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

        #region Drag Handling

        private void SetDragAnimation()
        {
            if (_playerStatus.PlayerState != PlayerState.Swapping) return;

            HandleCharacterScaling();

            var drag = _inputHandler.GetCurrentDrag();
            var forceScale = Mathf.Clamp01(drag.magnitude / _playerBallMovement.MaxForceMagnitude);
            AimCameraController.Instance.UpdateAim(drag, forceScale);
        }

        private void HandleCharacterScaling()
        {
            var dragForce = _inputHandler.GetCurrentDrag();
            var forceScale = Mathf.Clamp01(dragForce.magnitude / _playerBallMovement.MaxForceMagnitude);
            float currentYScale = Mathf.Lerp(1f, 0.8f, forceScale);
            float currentVelocityX = dragForce.x;

            if (_dragShakeTweener == null || !_dragShakeTweener.IsActive())
            {
                _dragShakeTweener = _playerStatus.CurrentGameObject.transform
                    .DOShakePosition(0.1f, forceScale * _shakeForce);
            }

            var currentScale = currentVelocityX < 0 ? 1f : -1f;
            _playerStatus.CurrentGameObject.transform.localScale = new Vector3(currentScale, currentYScale, 1f);
        }

        #endregion

        #region Event Handlers

        private void PlayerMovement_DragStarted()
        {
            AimCameraController.Instance.BeginAim();
            _playerAnimator.SetTrigger(DragStart);
            SlowDownGame();
            StartSweatEmission();
        }

        private void PlayerMovement_DragFinished(Vector2 _)
        {
            AimCameraController.Instance.ReleaseAim(punchy: true);
            ResetCharacterScale();
            ResetTimeScale();
            StopSweatEmission();
        }

        private void PlayerMovement_InputCancelled()
        {
            AimCameraController.Instance.ReleaseAim(punchy: false);
            ResetCharacterScale();
            ResetTimeScale();
            StopSweatEmission();
        }
        #endregion

        #region Reset Functions

        private void ResetCharacterScale()
        {
            float currentVelocityX = _playerMovement.GetVelocity().x;
            var currentScale = currentVelocityX < 0 ? 1f : -1f;
            _dragShakeTweener?.Kill();
            _playerStatus.CurrentGameObject.transform.localScale = new Vector3(currentScale, 1f, 1f);
            _lastHorizontalVelocity = currentVelocityX;
        }

        #endregion

        #region Utility Functions

        private void SlowDownGame()
        {
            if (_slowdownTween != null && _slowdownTween.IsActive())
            {
                _slowdownTween.Kill();
            }

            _slowdownTween = DOTween.To(
                () => 1f,
                x => TimeManager.Instance.SlowGame(x),
                0.1f,
                0.15f
            );
        }

        private void ResetTimeScale()
        {
            if (_slowdownTween != null && _slowdownTween.IsActive())
            {
                _slowdownTween.Kill();
            }

            DOTween.To(
                () => TimeManager.Instance.TimeSlow,
                x => TimeManager.Instance.SlowGame(x),
                1f,
                0.1f
            ).OnComplete(TimeManager.Instance.ResumeGame);
        }

        #endregion

        #region Swap Animations

        private void PlayerStatus_SwapToShooter()
        {
            PlaySwapToShooterAnimation();
        }

        private void PlayerStatus_SwapToBall()
        {
            PlaySwapToBallAnimation();
        }

        public void PlaySwapToBallAnimation()
        {
            _playerAnimator.SetTrigger(SwapToBall);
        }

        public void PlaySwapToShooterAnimation()
        {
            _playerAnimator.SetTrigger(SwapToShooter);
        }

        #endregion

        private void StartSweatEmission()
        {
            if (_sweatEmitRoutine == null)
            {
                _isEmittingSweat = true;
                _sweatEmitRoutine = StartCoroutine(EmitSweatParticlesRoutine());
            }
        }

        private void StopSweatEmission()
        {
            _isEmittingSweat = false;
            if (_sweatEmitRoutine != null)
            {
                StopCoroutine(_sweatEmitRoutine);
                _sweatEmitRoutine = null;
            }
        }

        private IEnumerator EmitSweatParticlesRoutine()
        {
            while (_isEmittingSweat)
            {
                var dragMagnitude = _inputHandler.GetCurrentDrag().magnitude;
                var forceMagnitude = Mathf.Min(dragMagnitude, _playerBallMovement.MaxForceMagnitude);
                var forceScale = forceMagnitude / _playerBallMovement.MaxForceMagnitude;
                var randomValue = Random.Range(0.4f, forceScale * 3f);
                _playerSweatParticleSystem.Emit((int)randomValue);
                yield return new WaitForSeconds(0.01f); // Adjust interval as needed
            }
        }
    }
}
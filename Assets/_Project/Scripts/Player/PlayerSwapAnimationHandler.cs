using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Cinemachine;
using Scripts.Camera;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

        [Header("Settings")] [SerializeField] private float _minSize;
        [SerializeField] private float _shakeForce;
        [SerializeField] private float _maxZoomOut = 1f;
        [SerializeField] private float _maxZoomAddOut = 1f;
        [SerializeField] private float _zoomDuration = 0.1f;

        private CinemachineVirtualCamera _virtualCamera;
        private float _originalSize;
        private float _lastHorizontalVelocity = 0f;

        private Tweener _dragShakeTweener;
        private TweenerCore<float, float, FloatOptions> _slowdownTween;
        private Tweener _zoomTween;

        private void Start()
        {
            _playerStatus.AnimationStateMachine().SetAnimator(_playerAnimator);
            _virtualCamera = CameraShakeHandler.Instance._virtualCamera;
            _originalSize = _virtualCamera.m_Lens.OrthographicSize;
        }

        private void OnEnable()
        {
            _playerStatus.OnSwapToBall += PlayerStatus_SwapToBall;
            _playerMovement.DragStarted += PlayerMovement_DragStarted;
            _playerMovement.DragFinished += PlayerMovement_DragFinished;
        }

        private void OnDisable()
        {
            _playerStatus.OnSwapToBall -= PlayerStatus_SwapToBall;
            _playerMovement.DragStarted -= PlayerMovement_DragStarted;
            _playerMovement.DragFinished -= PlayerMovement_DragFinished;
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
            HandleCameraZoom();
            EmitSweatParticles();
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

        private void HandleCameraZoom()
        {
            var dragForce = _inputHandler.GetCurrentDrag().magnitude;
            var forceScale = Mathf.Clamp01(dragForce / _playerBallMovement.MaxForceMagnitude);
            float targetSize = _originalSize + forceScale * _maxZoomOut;

            if (_zoomTween != null && _zoomTween.IsActive())
            {
                _zoomTween.Kill();
            }

            _zoomTween = DOTween.To(
                () => _virtualCamera.m_Lens.OrthographicSize,
                x => _virtualCamera.m_Lens.OrthographicSize = x,
                targetSize,
                _zoomDuration
            ).SetEase(Ease.OutQuad);
        }

        #endregion

        #region Event Handlers

        private void PlayerMovement_DragStarted()
        {
            _originalSize = _virtualCamera.m_Lens.OrthographicSize;
            _playerAnimator.SetTrigger(DragStart);
            SlowDownGame();
        }

        private void PlayerMovement_DragFinished(Vector2 _)
        {
            ResetCameraZoom();
            ResetCharacterScale();
            ResetTimeScale();
        }

        #endregion

        #region Reset Functions

        private void ResetCameraZoom()
        {
            if (_zoomTween != null && _zoomTween.IsActive())
            {
                _zoomTween.Kill();
            }

            _zoomTween = DOTween.To(
                () => _virtualCamera.m_Lens.OrthographicSize,
                x => _virtualCamera.m_Lens.OrthographicSize = x,
                _originalSize,
                _zoomDuration
            ).SetEase(Ease.OutQuad);

        }

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

        private void EmitSweatParticles()
        {
            var dragMagnitude = _inputHandler.GetCurrentDrag().magnitude;
            var forceMagnitude = Mathf.Min(dragMagnitude, _playerBallMovement.MaxForceMagnitude);
            var forceScale = forceMagnitude / _playerBallMovement.MaxForceMagnitude;

            var randomValue = Random.Range(0.4f, forceScale * 3f);
            _playerSweatParticleSystem.Emit((int)randomValue);
        }

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
    }
}
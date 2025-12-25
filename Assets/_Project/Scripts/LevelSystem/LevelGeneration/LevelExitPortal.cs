using System;
using DG.Tweening;
using Scripts.Player;
using Scripts.Player.InputHandling;
using UnityEngine;
using Zenject;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelExitPortal : MonoBehaviour
    {
        [Inject] private InputHandler _inputHandler;
        [Inject] private PlayerBallMovement _playerBallMovement;
        public event Action OnPortalTrigger;
        public event Action OnPlayerEnter;
        public event Action OnPortalClosed;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private float swirlDuration = 1f;
        [SerializeField] private float swirlScale = 0.1f;

        private bool _isSucking = false;
        private bool _isJumpingOut = false;

        private void Awake()
        {
            // Start with scale 0 and pop up
            transform.localScale = Vector3.zero;
            PlayPopUp();
        }

        public void PlayPopUp()
        {
            transform.DOScale(Vector3.one, swirlDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void PlayHide()
        {
            transform.DOScale(Vector3.zero, swirlDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                OnPortalClosed?.Invoke();
                Destroy(gameObject);
            });
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _playerBallMovement.HideTrail();
            // Pause input globally
            OnPortalTrigger?.Invoke();
            if (_isJumpingOut) return;
            if (other.TryGetComponent(out PlayerBallMovement player))
            {
                if (_isSucking) return;
                if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.LevelChange, false);
                _isSucking = true;
                player.StopMovement();
                Sequence swirlSequence = DOTween.Sequence();
                var playerTransform = player.transform;
                swirlSequence.Append(playerTransform.DOScale(swirlScale, swirlDuration)
                    .SetEase(Ease.InCirc).SetUpdate(true));
                swirlSequence.Join(playerTransform.DORotate(new Vector3(0, 0, 720f), swirlDuration,
                    RotateMode.FastBeyond360).SetUpdate(true));
                swirlSequence.Join(player.transform.DOMove(transform.position, swirlDuration * 0.8f)
                    .SetEase(Ease.Linear).SetUpdate(true));
                swirlSequence.SetUpdate(true);
                swirlSequence.OnComplete(() =>
                {
                    _isSucking = false;
                    PlayHide();
                    OnPlayerEnter?.Invoke();
                });
            }
        }
    }
}
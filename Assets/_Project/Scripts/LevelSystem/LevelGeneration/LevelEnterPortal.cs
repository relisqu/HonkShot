using System;
using DG.Tweening;
using Scripts.Player;
using Scripts.Player.InputHandling;
using UnityEngine;
using Zenject;
using Object = System.Object;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelEnterPortal : MonoBehaviour
    {
        [Inject] private InputHandler _inputHandler;


        [SerializeField] private Animator _animator;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private float swirlDuration = 1f;
        [SerializeField] private float swirlScale = 0.1f;

        private bool _finishedAnimation;

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
            Debug.Log("PlayHide");
            transform.DOScale(Vector3.zero, swirlDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                FindFirstObjectByType<PlayerBallMovement>().ResetTrail();
                Destroy(gameObject);
            });
        }


        public void PlayJumpOut(Transform player)
        {
            var transform = player.transform;
            Vector3 startScale = Vector3.one * swirlScale;
            Vector3 endScale = Vector3.one;
            float duration = swirlDuration;
            transform.localScale = startScale;
            transform.rotation = Quaternion.identity;
            Sequence jumpOutSequence = DOTween.Sequence();
            jumpOutSequence.Append(transform.DOScale(endScale, duration).SetEase(Ease.OutCirc).SetUpdate(true));
            jumpOutSequence.Join(transform.DORotate(new Vector3(0, 0, -1080f), duration, RotateMode.FastBeyond360)
                .SetUpdate(true));
            jumpOutSequence.SetUpdate(true);
            jumpOutSequence.OnComplete(() =>
            {
                _finishedAnimation = true;
                
                if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.LevelChange,true);
                PlayHide();
            });
        }

        public bool FinishedAnimation()
        {
            return _finishedAnimation;
        }
    }
}
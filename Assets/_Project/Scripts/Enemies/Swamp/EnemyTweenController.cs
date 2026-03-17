using System;
using DG.Tweening;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public enum TweenPriority
    {
        Low = 0,
        Normal = 10,
        High = 20,
        Override = 100
    }

    public class EnemyTweenController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _visualTransform;

        private Tweener _activeScaleTween;
        private TweenPriority _activeScalePriority;
        private Vector3 _baseScale;

        public Transform VisualTransform => _visualTransform;

        [SerializeField] private Animator _animator;

        private void Awake()
        {
            if (!_visualTransform)
                _visualTransform = transform;
            if (!_animator)
                _animator = GetComponent<Animator>();

            _baseScale = _visualTransform.localScale;
        }

        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
        }

        public Vector3 GetBaseScale()
        {
            return _baseScale;
        }

        public bool RequestScale(Vector3 target, float duration, Ease ease, TweenPriority priority, Action onComplete = null)
        {
            if (_activeScaleTween != null && _activeScaleTween.IsActive() && priority < _activeScalePriority)
                return false;

            KillActiveScale();
            SetAnimatorEnabled(false);

            _activeScalePriority = priority;
            _activeScaleTween = _visualTransform.DOScale(target, duration)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    _activeScaleTween = null;
                    onComplete?.Invoke();
                });

            return true;
        }

        public bool RequestPunchScale(Vector3 punch, float duration, TweenPriority priority, Action onComplete = null)
        {
            if (_activeScaleTween != null && _activeScaleTween.IsActive() && priority < _activeScalePriority)
                return false;

            KillActiveScale();

            _activeScalePriority = priority;
            _activeScaleTween = _visualTransform.DOPunchScale(punch, duration)
                .OnComplete(() =>
                {
                    _activeScaleTween = null;
                    onComplete?.Invoke();
                });

            return true;
        }

        public void ForceScale(Vector3 scale)
        {
            KillActiveScale();
            _visualTransform.localScale = scale;
            SetAnimatorEnabled(true);
        }

        public void ResetToBase()
        {
            KillActiveScale();
            _visualTransform.localScale = _baseScale;
            SetAnimatorEnabled(true);
        }

        public void SetAnimatorEnabled(bool enabled)
        {
            if (_animator)
                _animator.enabled = enabled;
        }

        private void KillActiveScale()
        {
            if (_activeScaleTween != null && _activeScaleTween.IsActive())
                _activeScaleTween.Kill();
            _activeScaleTween = null;
        }

        private void OnDestroy()
        {
            KillActiveScale();
        }
    }
}

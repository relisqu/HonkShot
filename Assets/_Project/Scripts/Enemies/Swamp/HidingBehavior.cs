using System;
using System.Collections;
using DG.Tweening;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class HidingBehavior : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float _visibleDuration = 3f;
        [SerializeField] private float _hiddenDuration = 2f;
        [SerializeField] private float _initialDelay = 1f;

        [Header("Hiding Settings")]
        [SerializeField] private bool _disableColliderWhenHidden = true;
        [SerializeField] private float _hiddenXScale = 0.1f;
        [SerializeField] private float _scaleTransitionDuration = 0.3f;

        [Header("Components")]
        [SerializeField] private HealthController _healthController;
        [SerializeField] private Collider2D _mainCollider;
        [SerializeField] private HidingShootingEnemyStateMachine _stateMachine;
        [SerializeField] private EnemyTweenController _tweenController;

        private Coroutine _cycleRoutine;
        private Vector3 _originalScale;

        public bool IsHidden => _stateMachine && _stateMachine.IsHidden;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
            if (!_mainCollider)
                _mainCollider = GetComponent<Collider2D>();
            if (!_stateMachine)
                _stateMachine = GetComponent<HidingShootingEnemyStateMachine>();
            if (!_tweenController)
                _tweenController = GetComponent<EnemyTweenController>();

            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            _cycleRoutine = StartCoroutine(HideCycleRoutine());
        }

        private void OnDisable()
        {
            if (_cycleRoutine != null)
                StopCoroutine(_cycleRoutine);

            ForceShow();
        }

        private IEnumerator HideCycleRoutine()
        {
            yield return new WaitForSeconds(_initialDelay);

            while (true)
            {
                yield return new WaitForSeconds(_visibleDuration);
                yield return StartCoroutine(HideRoutine());
                yield return new WaitForSeconds(_hiddenDuration);
                yield return StartCoroutine(ShowRoutine());
            }
        }

        private IEnumerator HideRoutine()
        {
            if (!_stateMachine.TrySetState(EnemyState.Hiding))
                yield break;

            var hiddenScale = new Vector3(_originalScale.x, _hiddenXScale * _originalScale.y, _originalScale.z);
            _tweenController.RequestScale(hiddenScale, _scaleTransitionDuration, Ease.InBack, TweenPriority.High);

            yield return new WaitForSeconds(_scaleTransitionDuration);

            if (_healthController)
                _healthController.SetInvincible((int)InvincibilityEnum.Hiding, true);

            if (_disableColliderWhenHidden && _mainCollider)
                _mainCollider.enabled = false;

            _stateMachine.TrySetState(EnemyState.Hidden);
        }

        private IEnumerator ShowRoutine()
        {
            if (!_stateMachine.TrySetState(EnemyState.Showing))
                yield break;

            if (_healthController)
                _healthController.SetInvincible((int)InvincibilityEnum.Hiding, false);

            if (_disableColliderWhenHidden && _mainCollider)
                _mainCollider.enabled = true;

            _tweenController.RequestScale(_originalScale, _scaleTransitionDuration, Ease.OutBack, TweenPriority.High);

            yield return new WaitForSeconds(_scaleTransitionDuration);

            _tweenController.SetAnimatorEnabled(true);
            _stateMachine.TrySetState(EnemyState.Active);
        }

        private void ForceShow()
        {
            if (_stateMachine && !_stateMachine.IsVisible)
                _stateMachine.TrySetState(EnemyState.Active);

            if (_healthController)
                _healthController.SetInvincible((int)InvincibilityEnum.Hiding, false);

            if (_disableColliderWhenHidden && _mainCollider)
                _mainCollider.enabled = true;

            if (_tweenController)
                _tweenController.ForceScale(_originalScale);
        }
    }
}

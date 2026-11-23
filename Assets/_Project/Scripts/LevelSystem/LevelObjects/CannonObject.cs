using Scripts.Other;
using System.Collections;
using System.Collections.Generic;
using Scripts.Health;
using Scripts.LevelSystem.LevelObjects.Interaction;
using UnityEngine;
using DG.Tweening;
using Scripts.Player.InputHandling;
using Scripts.PointSystem;
using Zenject;

namespace Scripts.LevelSystem.LevelObjects
{
    public class Cannon : MonoBehaviour
    {
        [Inject] private InputHandler _inputHandler;
        [Header("Rotation")] [SerializeField] private List<Transform> _rotatingParts;
        [SerializeField] private float rotateAngle = 60f;
        [SerializeField] private float rotateSpeed = 2f;

        [Header("Shooting")] [SerializeField] private float pauseTime = 0.5f;
        [SerializeField] private float shootForce = 10f;
        [SerializeField] private AttackPointsObject _attackPointsObject;

        [Tooltip("How long a just-shot object is ignored while it is still leaving the barrel. " +
                 "Re-enabling its collider inside the trigger raises OnTriggerEnter2D again, which " +
                 "would immediately recapture it.")]
        [SerializeField] private float _recaptureCooldown = 0.3f;

        private float _startRotation;
        private float _targetRotation;
        private float _rotationTimer;

        private bool _rotatingForward = true;
        private CannonInteractable _containedObject;
        private CannonInteractable _lastShotObject;
        private float _lastShotTime;

        private float _angle = 0f;
        private float _direction = 1f;
        private Vector3 _originalScale;

        private void Awake()
        {
            _startRotation = transform.eulerAngles.z;
            if (_rotatingParts is { Count: > 0 })
                _originalScale = _rotatingParts[0].localScale;
        }

        private void Update()
        {
            RotateCannon();
        }

        private void RotateCannon()
        {
            if (_containedObject) return;
            _angle += _direction * rotateSpeed * Time.deltaTime;
            if (_angle > rotateAngle / 2f || _angle < -rotateAngle / 2f)
            {
                _angle = Mathf.Clamp(_angle, -rotateAngle / 2f, rotateAngle / 2f);
                _direction *= -1f;
            }

            // Apply relative rotation to base cannon rotation
            foreach (var rotatingPart in _rotatingParts)
            {
                rotatingPart.localRotation = Quaternion.Euler(0f, 0f, _angle);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_containedObject) return;

            if (!other.TryGetComponentInBody(out CannonInteractable interactable)) return;

            // OnExitCannon re-enables the collider while it is still inside this trigger, so Unity
            // raises another enter on the next physics step. Without this the cannon swallows the
            // object again the moment it was shot.
            if (interactable == _lastShotObject && Time.time - _lastShotTime < _recaptureCooldown) return;

            _containedObject = interactable;
            StartCoroutine(HandleCannon(interactable));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Left the barrel for real, so it may be captured again straight away.
            if (other.TryGetComponentInBody(out CannonInteractable interactable) && interactable == _lastShotObject)
                _lastShotObject = null;
        }

        private void OnDisable()
        {
            if (!_containedObject) return;

            // HandleCannon owns the player's visibility, invincibility and input. If this cannon is
            // disabled mid-cycle the coroutine dies and nothing else would ever give those back.
            _containedObject.OnExitCannon(Vector2.up, 0f);
            _containedObject = null;
        }

        private IEnumerator HandleCannon(CannonInteractable interactable)
        {
            if (interactable is PlayerCannonInteractable)
            {
                _attackPointsObject.EarnTouchPoints();
                if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, false);
            }

            if (_rotatingParts is { Count: > 0 })
                foreach (var rotatingPart in _rotatingParts)
                {
                    rotatingPart
                        .DOScale(new Vector3(_originalScale.x * 1.2f, _originalScale.y * 0.7f, _originalScale.z), 0.15f)
                        .SetEase(Ease.OutQuad);
                }

            interactable.OnEnterCannon();

            yield return new WaitForSeconds(pauseTime);

            Vector2 shootDirection = _rotatingParts[0].up;

            _lastShotObject = interactable;
            _lastShotTime = Time.time;
            interactable.OnExitCannon(shootDirection, shootForce);

            if (_rotatingParts is { Count: > 0 })
                foreach (var rotatingPart in _rotatingParts)
                {
                    rotatingPart.DOScale(_originalScale, 0.25f).SetEase(Ease.OutBack);
                }

            if (interactable is PlayerCannonInteractable)
            {
                if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, true);
            }

            _containedObject = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (_rotatingParts is { Count: > 0 }) return;

            Gizmos.color = Color.yellow;
            Vector3 pos = _rotatingParts[0].position;

            Quaternion left = Quaternion.Euler(0, 0, -rotateAngle / 2f);
            Quaternion right = Quaternion.Euler(0, 0, rotateAngle / 2f);

            Vector3 leftDir = (transform.rotation * left) * Vector3.up;
            Vector3 rightDir = (transform.rotation * right) * Vector3.up;

            Gizmos.DrawRay(pos, leftDir * 1.5f);
            Gizmos.DrawRay(pos, rightDir * 1.5f);
        }
    }
}
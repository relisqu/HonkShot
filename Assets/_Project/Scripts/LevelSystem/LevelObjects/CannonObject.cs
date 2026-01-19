using System.Collections;
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
        [Header("Rotation")] [SerializeField] private Transform rotatingPart;
        [SerializeField] private float rotateAngle = 60f;
        [SerializeField] private float rotateSpeed = 2f;

        [Header("Shooting")] [SerializeField] private float pauseTime = 0.5f;
        [SerializeField] private float shootForce = 10f;
        [SerializeField] private AttackPointsObject _attackPointsObject;

        private float _startRotation;
        private float _targetRotation;
        private float _rotationTimer;

        private bool _rotatingForward = true;
        private CannonInteractable _containedObject;

        private float _angle = 0f;
        private float _direction = 1f;
        private Vector3 _originalScale;

        private void Awake()
        {
            _startRotation = transform.eulerAngles.z;
            if (rotatingPart != null)
                _originalScale = rotatingPart.localScale;
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
            rotatingPart.localRotation = Quaternion.Euler(0f, 0f, _angle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_containedObject != null) return;

            if (other.TryGetComponent(out CannonInteractable interactable))
            {
                _containedObject = interactable;
                StartCoroutine(HandleCannon(interactable));
            }
        }

        private IEnumerator HandleCannon(CannonInteractable interactable)
        {
            if (interactable is PlayerCannonInteractable)
            {
                _attackPointsObject.EarnTouchPoints();
                if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, false);
            }

            if (rotatingPart)
                rotatingPart
                    .DOScale(new Vector3(_originalScale.x * 1.2f, _originalScale.y * 0.7f, _originalScale.z), 0.15f)
                    .SetEase(Ease.OutQuad);

            interactable.OnEnterCannon();

            yield return new WaitForSeconds(pauseTime);

            Vector2 shootDirection = rotatingPart.up;
            Debug.Log(shootDirection);
            interactable.OnExitCannon(shootDirection, shootForce);

            if (rotatingPart)
                rotatingPart.DOScale(_originalScale, 0.25f).SetEase(Ease.OutBack);

            if (interactable is PlayerCannonInteractable)
            {
                
                if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.Cannon, true);
            }

            _containedObject = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (rotatingPart == null) return;

            Gizmos.color = Color.yellow;
            Vector3 pos = rotatingPart.position;

            Quaternion left = Quaternion.Euler(0, 0, -rotateAngle / 2f);
            Quaternion right = Quaternion.Euler(0, 0, rotateAngle / 2f);

            Vector3 leftDir = (transform.rotation * left) * Vector3.up;
            Vector3 rightDir = (transform.rotation * right) * Vector3.up;

            Gizmos.DrawRay(pos, leftDir * 1.5f);
            Gizmos.DrawRay(pos, rightDir * 1.5f);
        }
    }
}
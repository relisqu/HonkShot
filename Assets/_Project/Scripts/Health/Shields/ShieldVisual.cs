using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Scripts.Health
{
    public class ShieldVisual : MonoBehaviour
    {
        [Header("Shield Sprites")] [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField] private Sprite[] _sprites;

        [Header("Oval Movement")] [SerializeField]
        private float _ovalWidth = 2f;

        [SerializeField] private float _ovalHeight = 1.5f;
        [SerializeField] private float _rotationSpeed = 60f; // degrees per second
        [SerializeField] private float _pulseSpeed = 1f;
        [SerializeField] private float _pulseIntensity = 0.1f;


        private Transform _playerTransform;
        private Vector3 _originalLocalPosition;
        private float _currentAngle = 0f;
        private bool _isActive = true;

        void Start()
        {
            // Get player transform
            _playerTransform = transform.parent;
            if (_playerTransform == null)
            {
                _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            }

            _originalLocalPosition = transform.localPosition;

            // Start rotation and pulse
            StartCoroutine(RotateShield());
            StartCoroutine(PulseShield());
        }

        private IEnumerator RotateShield()
        {
            while (_isActive)
            {
                // Calculate oval position
                float x = Mathf.Cos(_currentAngle * Mathf.Deg2Rad) * _ovalWidth * 0.5f;
                float y = Mathf.Sin(_currentAngle * Mathf.Deg2Rad) * _ovalHeight * 0.5f;

                // Update position
                transform.localPosition = _originalLocalPosition + new Vector3(x, y, 0);

                // Update sprite based on direction
                UpdateShieldSprite();


                // Rotate angle
                _currentAngle += _rotationSpeed * Time.deltaTime;
                if (_currentAngle >= 360f)
                    _currentAngle -= 360f;

                yield return null;
            }
        }

        private IEnumerator PulseShield()
        {
            Vector3 originalScale = transform.localScale;

            while (_isActive)
            {
                float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseIntensity;
                transform.localScale = originalScale * pulse;

                yield return null;
            }
        }

        private void UpdateShieldSprite()
        {
            if (_spriteRenderer == null || _sprites == null || _sprites.Length == 0) return;

            // Calculate sprite index based on angle
            int spriteIndex = CalculateSpriteIndex(_currentAngle);

            // Ensure index is within bounds
            spriteIndex = Mathf.Clamp(spriteIndex, 0, _sprites.Length - 1);

            _spriteRenderer.sprite = _sprites[spriteIndex];
        }

        private int CalculateSpriteIndex(float angle)
        {
            if (_sprites == null || _sprites.Length == 0) return 0;

            var newAngle = (_currentAngle + 90);

            return (int)newAngle / 360 * (_sprites.Length - 1);
        }


        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                gameObject.SetActive(false);
            }
        }

        public void PlayDestructionAnimation()
        {
            _isActive = false;

            if (_spriteRenderer)
            {
                _spriteRenderer.DOFade(0f, 0.3f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => Destroy(gameObject));

                transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            _isActive = false;
        }
    }
}
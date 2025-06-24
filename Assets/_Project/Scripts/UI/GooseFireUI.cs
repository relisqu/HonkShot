using UnityEngine;

namespace Scripts.UI
{
    public class GooseFireUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _fireImage;
        [SerializeField] private Scripts.Player.GooseFireSystem _gooseFireSystem;

        [Header("Y Position Settings")]
        [SerializeField] private float _minY = -266f;
        [SerializeField] private float _maxY = 0f;
        [SerializeField] private float _moveSpeed = 10f;

        private float _targetY;

        private void Awake()
        {
            if (_gooseFireSystem == null)
                _gooseFireSystem = FindObjectOfType<Scripts.Player.GooseFireSystem>();
        }

        private void OnEnable()
        {
            if (_gooseFireSystem != null)
                _gooseFireSystem.FireChanged += OnFireChanged;
        }

        private void OnDisable()
        {
            if (_gooseFireSystem != null)
                _gooseFireSystem.FireChanged -= OnFireChanged;
        }

        private void OnFireChanged(float fire)
        {
            float t = fire / _gooseFireSystem.MaxFire;
            _targetY = Mathf.Lerp(_minY, _maxY, t);
        }

        private void Update()
        {
            if (_fireImage)
            {
                Vector2 pos = _fireImage.anchoredPosition;
                pos.y = Mathf.Lerp(pos.y, _targetY, Time.deltaTime * _moveSpeed);
                _fireImage.anchoredPosition = pos;
            }
        }
    }
} 
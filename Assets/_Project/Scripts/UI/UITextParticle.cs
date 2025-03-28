using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class UITextParticle : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _speed;
        [SerializeField] private float _fadeSpeed;

        private bool _isShown;

        private void Awake()
        {
            _isShown = false;
            _canvas.worldCamera = UnityEngine.Camera.current;
            _canvasGroup.alpha = 0;
        }

        public void Update()
        {
            if (!_isShown) return;
            transform.position += (Vector3)Vector2.up * (Time.deltaTime * _speed);
        }

        public void SetScale(float scale)
        {
            transform.localScale = Mathf.Clamp(scale, 0.8f, 1f) * Vector3.one;
        }

        public void Show()
        {
            _isShown = true;

            _canvasGroup.alpha = 1;
            _canvasGroup.DOFade(0, _fadeSpeed).SetEase(Ease.InCubic).OnComplete(() => { Destroy(gameObject); });
        }

        public void ShowText(string text)
        {
            _text.SetText(text);
            Show();
        }
    }
}
using System;
using DG.Tweening;
using Scripts.Progress;
using UnityEngine;

namespace Scripts.UI
{
    public class BlackViewEffect : MonoBehaviour
    {
        public static BlackViewEffect Instance;

        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            Instance = this;
        }

        public void HideVignette(float duration)
        {
            _canvasGroup.DOFade(0f, duration);
        }

        public void ShowVignette(float duration)
        {
            _canvasGroup.DOFade(1f, duration);
        }
    }
}
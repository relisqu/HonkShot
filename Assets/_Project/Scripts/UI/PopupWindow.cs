using System;
using DG.Tweening;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.InputHandling;
using Scripts.Services.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI
{
    public class PopupWindow : MonoBehaviour
    {
        private ILocalizationService _localizationService;

        [Inject] private InputHandler _inputHandler;
        [SerializeField] private GameObject _popupWindowGameObject;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _okButton;

        [Inject]
        private void Construct(LocalizationService localizationService, InputHandler inputHandler)
        {
            _inputHandler = inputHandler;
            _localizationService = localizationService;
        }

        private void OkButton_Click()
        {
            Hide();
        }

        public void Init()
        {
        }

        public void SetText(string titleKey, string descriptionKey)
        {
            _title.text = _localizationService.Get(titleKey);
            _description.text = _localizationService.Get(descriptionKey);
        }

        public void Show()
        {
            Debug.Log($"Showing {_title.text}");
            DOTween.defaultTimeScaleIndependent = true;
            _canvasGroup.DOFade(1f, 0.2f);
            BlackViewEffect.Instance.ShowVignette(0.2f);
            _popupWindowGameObject.transform.DOPunchScale(Vector3.one, 0.2f, 10, 1);
            DOTween.defaultTimeScaleIndependent = false;
            TimeManager.Instance.PauseGame();
            _inputHandler.SetInputEnabled(InputLayer.Tutorial, false);
        }

        public void Hide()
        {
            DOTween.defaultTimeScaleIndependent = true;
            BlackViewEffect.Instance.HideVignette(0.2f);
            _canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
            {
                _canvasGroup.blocksRaycasts = false;
                TimeManager.Instance.ResumeGame();
                _inputHandler.SetInputEnabled(InputLayer.Tutorial, true);
            });
            DOTween.defaultTimeScaleIndependent = true;
        }


        private void Awake()
        {
            _okButton.onClick.AddListener(OkButton_Click);
        }

        private void OnDestroy()
        {
            _okButton.onClick.RemoveListener(OkButton_Click);
        }
    }
}
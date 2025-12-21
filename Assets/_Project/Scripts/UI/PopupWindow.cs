using System;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI
{
    public class PopupWindow : MonoBehaviour
    {
        [Inject] private InputHandler _inputHandler;
        [Inject] private TimeManager _timeManager;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Button _okButton;

        private void OkButton_Click()
        {
            Hide();
        }

        public void Init()
        {
        }

        public void Show()
        {
            _canvasGroup.alpha = 1;
            _timeManager.PauseGame();
        }

        public void Hide()
        {
            _timeManager.ResumeGame();
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        }


        private void Awake()
        {
            _okButton.onClick.AddListener(OkButton_Click);
        }

        private void OnDestroy()
        {
        }
    }
}
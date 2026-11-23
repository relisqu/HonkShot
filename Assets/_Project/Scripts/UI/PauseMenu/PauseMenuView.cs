using DG.Tweening;
using Scripts.Other;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Scripts.UI.PauseMenu
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private InputActionReference _pauseActionReference;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.15f;

        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private SettingsMenuView _settingsMenuView;

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _restartButton;

        private InputHandler _inputHandler;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        [Inject]
        private void Construct(InputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }

        private void Awake()
        {
            _resumeButton.onClick.AddListener(ResumeButton_Click);
            _settingsButton.onClick.AddListener(SettingsButton_Click);
            _restartButton.onClick.AddListener(RestartButton_Click);

            _settingsMenuView.gameObject.SetActive(false);
            SetInteractable(false);
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            _pauseActionReference.action.Enable();
            _pauseActionReference.action.performed += PauseAction_Performed;
            _settingsMenuView.OnClosed += SettingsMenuView_Closed;
        }

        private void OnDisable()
        {
            _pauseActionReference.action.performed -= PauseAction_Performed;
            _settingsMenuView.OnClosed -= SettingsMenuView_Closed;
        }

        private void OnDestroy()
        {
            _resumeButton.onClick.RemoveListener(ResumeButton_Click);
            _settingsButton.onClick.RemoveListener(SettingsButton_Click);
            _restartButton.onClick.RemoveListener(RestartButton_Click);
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;

            // Input goes first: cancelling an aim kicks off its own time-scale tween, which the menu
            // pause then holds back until the menu closes.
            _inputHandler.SetInputEnabled(InputLayer.PauseMenu, false);
            TimeManager.Instance.SetMenuPaused(true);

            ShowMainPanel(_resumeButton);
            SetInteractable(true);
            Fade(1f);
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (_settingsMenuView.IsOpen)
                _settingsMenuView.Hide();

            SetInteractable(false);
            Fade(0f);

            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(null);

            TimeManager.Instance.SetMenuPaused(false);
            _inputHandler.SetInputEnabled(InputLayer.PauseMenu, true);
        }

        private void PauseAction_Performed(InputAction.CallbackContext context)
        {
            if (!_isOpen)
                Open();
            else if (_settingsMenuView.IsOpen)
                _settingsMenuView.Hide();
            else
                Close();
        }

        private void ResumeButton_Click() =>
            Close();

        private void SettingsButton_Click()
        {
            _mainPanel.SetActive(false);
            _settingsMenuView.Show();
        }

        private void RestartButton_Click() =>
            SceneRestarter.RestartCurrentScene();

        private void SettingsMenuView_Closed()
        {
            if (_isOpen)
                ShowMainPanel(_settingsButton);
        }

        private void ShowMainPanel(Selectable selected)
        {
            _mainPanel.SetActive(true);

            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }

        private void SetInteractable(bool value)
        {
            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }

        private void Fade(float alpha)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(alpha, _fadeDuration).SetUpdate(true);
        }
    }
}

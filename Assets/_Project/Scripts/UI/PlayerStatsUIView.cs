using System;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using Scripts.Player;
using Scripts.Player.Dash;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class PlayerStatsUIView : MonoBehaviour
    {
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Image _dashPrefab;

        [SerializeField] private List<Image> _dashViews = new List<Image>();

        [SerializeField] private PlayerDashController _playerDashController;
        private HealthController _playerHealthController;

        private Color _defaultColor;

        private void Start()
        {
            var player = FindObjectOfType<PlayerMovement>();
            _playerHealthController = player.GetComponent<HealthController>();
            var dashes = _playerDashController.MaxDashCount;

            foreach (var dashView in _dashViews)
            {
                dashView.color = Color.clear;
            }

            PlayerDashController_UpdatedDashCount();
            _playerDashController.UpdatedDashCount += PlayerDashController_UpdatedDashCount;
        }

        private void OnDestroy()
        {
            _playerDashController.UpdatedDashCount -= PlayerDashController_UpdatedDashCount;
        }

        private void PlayerDashController_UpdatedDashCount()
        {
            var currentDashCount = _playerDashController.CurrentDashCount;

            for (int i = 0; i < _dashViews.Count; i++)
            {
                if (i < currentDashCount)
                {
                    if (_dashViews[i].color.a > 0)
                    {
                    }
                    else
                    {
                        _dashViews[i].DOColor(Color.white, 0.1f);
                    }
                }
                else
                {
                    if (_dashViews[i].color.a > 0)
                    {
                        _dashViews[i].DOColor(Color.clear, 0.1f);
                    }
                    else
                    {
                    }
                }
            }
        }


        private void Update()
        {
            _hpSlider.value = _playerHealthController.RemainingHealthPercentage;
        }
    }
}
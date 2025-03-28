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
        [SerializeField] private Transform _dashesTransform;
        [SerializeField] private Image _dashPrefab;

        private List<Image> _dashViews = new List<Image>();

        private PlayerDashController _playerDashController;
        private HealthController _playerHealthController;

        private Color _defaultColor;

        private void Start()
        {
            var player = FindObjectOfType<PlayerMovement>();
            _playerDashController = player.GetComponent<PlayerDashController>();
            _playerHealthController = player.GetComponent<HealthController>();
            var dashes = _playerDashController.MaxDashCount;

            for (int i = 0; i < dashes; i++)
            {
                var dash = Instantiate(_dashPrefab, _dashesTransform);
                _dashViews.Add(dash);
                _defaultColor = dash.color;
                dash.color = Color.clear;
            }

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
                        _dashViews[i].DOColor(_defaultColor, 0.1f);
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
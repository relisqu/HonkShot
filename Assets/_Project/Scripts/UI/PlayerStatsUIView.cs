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

        [SerializeField] private Transform _dashViewsTransform;
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private Vector3 _dashViewsOffset;
        [SerializeField] private List<Image> _dashViews = new List<Image>();

        [SerializeField] private PlayerDashController _playerDashController;
        private HealthController _playerHealthController;

        private Color _defaultColor;

        private void Start()
        {
            var player = FindObjectOfType<PlayerMovement>();
            _playerHealthController = player.GetComponent<HealthController>();
            var dashes = _playerDashController.MaxDashCount;

            for (int i = 0; i < _playerDashController.MaxDashCount; i++)
            {
                var dashView = Instantiate(_dashPrefab, _dashViewsTransform);
                if (i == 0)
                {
                    dashView.transform.localPosition = _startPosition;
                }
                else
                {
                    dashView.transform.localPosition = _dashViews[i - 1].transform.localPosition + _dashViewsOffset;
                }

                _dashViews.Add(dashView);
            }

            foreach (var dashView in _dashViews)
            {
                dashView.color = Color.clear;
            }

            PlayerDashController_UpdatedDashCount();
            _playerDashController.UpdatedDashCount += PlayerDashController_UpdatedDashCount;
            _playerDashController.UpdatedMaxDashCount += PlayerDashController_UpdatedMaxDashCount;
        }

        private void OnDestroy()
        {
            _playerDashController.UpdatedDashCount -= PlayerDashController_UpdatedDashCount;
            _playerDashController.UpdatedMaxDashCount -= PlayerDashController_UpdatedMaxDashCount;
        }

        private void PlayerDashController_UpdatedMaxDashCount()
        {
            if (_playerDashController.MaxDashCount < _dashViews.Count)
            {
                while (_playerDashController.MaxDashCount < _dashViews.Count)
                {
                    Destroy(_dashViews[^1].gameObject);
                    _dashViews.RemoveAt(_dashViews.Count - 1);
                }

                Debug.Log("[PlayerStatsUIView] Destroyed dash images:  " + _playerDashController.MaxDashCount);
            }

            else if (_playerDashController.MaxDashCount > _dashViews.Count)
            {
                while (_playerDashController.MaxDashCount > _dashViews.Count)
                {
                    var dashView = Instantiate(_dashPrefab, _dashViewsTransform);
                    if (_playerDashController.MaxDashCount == 0)
                    {
                        dashView.transform.localPosition = _dashViewsOffset;
                    }
                    else
                    {
                        dashView.transform.localPosition =
                            _dashViews[^1].transform.localPosition + _dashViewsOffset;
                    }

                    dashView.DOColor(Color.clear, 0.1f);
                    _dashViews.Add(dashView);
                }

                Debug.Log("[PlayerStatsUIView] Added dash images:  " + _playerDashController.MaxDashCount);
            }
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
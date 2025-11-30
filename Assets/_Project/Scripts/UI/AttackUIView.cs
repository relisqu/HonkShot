using System;
using DG.Tweening;
using Scripts.PointSystem;
using TMPro;
using UnityEngine;
using Zenject;
using PointReceiver = Scripts.PointSystem.PointReceiver;

namespace Scripts.UI
{
    public class AttackUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currentScoreText;
        [SerializeField] private TMP_Text _maxScoreText;

        [Inject] private PointReceiver _pointReceiver;

        private Tweener _tweener;

        private void PointReceiver_ChangedMaxPoints()
        {
            Debug.Log("Max points changed");
            if (_tweener == null)
            {
                _tweener = _maxScoreText.transform.DOPunchScale(0.2f * Vector3.one, 0.1f).OnComplete(() =>
                {
                    _tweener = null;
                });
            }

            var pointReceiver = _pointReceiver ?? PointReceiver.Instance; // Fallback to Instance if injection failed
            if (pointReceiver != null)
            {
                _maxScoreText.text = (int)(pointReceiver.MaxPointsCount) + "";
            }
        }

        void PointReceiver_ChangedCurrentPoints()
        {
            Debug.Log("PointReceiver_ChangedCurrentPoints");
            var pointReceiver = _pointReceiver ?? PointReceiver.Instance; // Fallback to Instance if injection failed
            if (pointReceiver != null)
            {
                _currentScoreText.text = (int)(pointReceiver.GetAttackPoints()) + "";
            }
        }

        private void Start()
        {
            var pointReceiver = _pointReceiver ?? PointReceiver.Instance; // Fallback to Instance if injection failed
            if (pointReceiver != null)
            {
                pointReceiver.ChangedCurrentPoints += PointReceiver_ChangedCurrentPoints;
                pointReceiver.ChangedMaxPoints += PointReceiver_ChangedMaxPoints;
                _currentScoreText.text = (int)(pointReceiver.GetAttackPoints()) + "";
            }
        }

        private void OnDestroy()
        {
            var pointReceiver = _pointReceiver ?? PointReceiver.Instance; // Fallback to Instance if injection failed
            if (pointReceiver != null)
            {
                pointReceiver.ChangedCurrentPoints -= PointReceiver_ChangedCurrentPoints;
                pointReceiver.ChangedMaxPoints -= PointReceiver_ChangedMaxPoints;
            }
        }
    }
}
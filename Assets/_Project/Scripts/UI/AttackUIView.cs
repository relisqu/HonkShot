using System;
using DG.Tweening;
using Scripts.PointSystem;
using TMPro;
using UnityEngine;
using PointReceiver = Scripts.PointSystem.PointReceiver;

namespace Scripts.UI
{
    public class AttackUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currentScoreText;
        [SerializeField] private TMP_Text _maxScoreText;

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

            _maxScoreText.text = (int)(PointReceiver.Instance.MaxPointsCount) + "";
        }

        void PointReceiver_ChangedCurrentPoints()
        {
            Debug.Log("PointReceiver_ChangedCurrentPoints");
            _currentScoreText.text = (int)(PointReceiver.Instance.GetAttackPoints()) + "";
        }

        private void Start()
        {
            PointReceiver.Instance.ChangedCurrentPoints += PointReceiver_ChangedCurrentPoints;
            PointReceiver.Instance.ChangedMaxPoints += PointReceiver_ChangedMaxPoints;
            _currentScoreText.text = (int)(PointReceiver.Instance.GetAttackPoints()) + "";
        }

        private void OnDestroy()
        {
            PointReceiver.Instance.ChangedCurrentPoints -= PointReceiver_ChangedCurrentPoints;
            PointReceiver.Instance.ChangedMaxPoints -= PointReceiver_ChangedMaxPoints;
        }
    }
}
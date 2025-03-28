using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.PointSystem
{
    public class AttackPointsObject : MonoBehaviour
    {
        [FormerlySerializedAs("_pointObjectType")] [SerializeField]
        private AttackPointsObjectType attackPointsObjectType;

        [FormerlySerializedAs("PointsPerTouchCount")] [SerializeField]
        private int _pointsPerTouchCount;

        [Tooltip("How much points player gets for 1 second of stay inside of object")] [SerializeField]
        private int PointsPerStayCount;

        private float _stayPoints;

        public void IncreaseStayPoints()
        {
            Debug.Log($"StayPoints: +{PointsPerStayCount * Time.deltaTime}");
            _stayPoints += PointsPerStayCount * Time.deltaTime;
        }

        public void EarnStayPoints()
        {
            if (_stayPoints != 0)
            {
                Debug.Log($"AddPoints stay: +{_stayPoints}");
                PointReceiver.Instance.AddPoints(attackPointsObjectType, _stayPoints);
                _stayPoints = 0;
            }
        }

        public void EarnTouchPoints()
        {
            if (_pointsPerTouchCount != 0)
            {
                Debug.Log($"AddPoints touch: +{_pointsPerTouchCount}");
                PointReceiver.Instance.AddPoints(attackPointsObjectType, _pointsPerTouchCount);
            }
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out PointReceiver _))
            {
                EarnTouchPoints();
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out PointReceiver _))
            {
                EarnTouchPoints();
            }
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out PointReceiver _))
            {
                _stayPoints += PointsPerStayCount * Time.deltaTime;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PointReceiver pointReceiver))
            {
                EarnStayPoints();
            }
        }
    }
}
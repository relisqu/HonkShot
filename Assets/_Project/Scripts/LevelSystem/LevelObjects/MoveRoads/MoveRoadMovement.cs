using System;
using Scripts.Player;
using Scripts.PointSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace Scripts.LevelObjects.MoveRoads
{
    public class MoveRoadMovement : MonoBehaviour
    {
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] public float _speed = 1.2f;
        [SerializeField] private AttackPointsObject _attackPointsObject;

        [FormerlySerializedAs("rotationSpeed")]
        public float _rotationSpeed = 5f;

        public float _playerDistance = 1f;
        public float _addedForce = 1f;

        [FormerlySerializedAs("rotationSpeed")]
        private PlayerMovement _playerMovement;

        private bool _wasInRoadBeforeDrag;

        private void PlayerMovement_DragFinished(Vector2 obj)
        {
            if (_wasTouched) _wasInRoadBeforeDrag = true;
        }


        private Vector2 _closestPoint;
        private float _closestPointT;

        private bool _wasTouched;

        private void Update()
        {
            float3 closestPlayerPoint;
            float closestPlayerPointT;

            SplineUtility.GetNearestPoint(_splineContainer.Spline,
                _playerMovement.transform.position - transform.position,
                out closestPlayerPoint, out closestPlayerPointT);
            var position =
                new Vector2(closestPlayerPoint.x, closestPlayerPoint.y) + (Vector2)transform.position;
            _closestPoint = position;
            _closestPointT = closestPlayerPointT;
            var distance = Vector2.Distance(position,
                _playerMovement.transform.position);

            if (distance < _playerDistance)
            {
                if (_wasInRoadBeforeDrag) return;
                if (_wasTouched)
                {
                    _attackPointsObject.IncreaseStayPoints();
                }
                else
                {
                    _attackPointsObject.EarnTouchPoints();
                    _wasTouched = true;
                }

                var angle = _splineContainer.Spline.EvaluateTangent(closestPlayerPointT);
                ApplyForce(_playerMovement.GetRigidbody(), angle);
            }
            else
            {
                if (_wasTouched)
                {
                    _attackPointsObject.EarnStayPoints();
                    _wasTouched = false;
                    _wasInRoadBeforeDrag = false;
                }
            }
        }

        Vector2 RotateVector(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        public void ApplyForce(Rigidbody2D rigidbody2D, float3 angle)
        {
            Vector3 insideForce;
            if (_closestPointT < 0.95f)
            {
                insideForce =
                    new Vector3(_closestPoint.x, _closestPoint.y, _closestPointT) - rigidbody2D.transform.position;
                if (insideForce.magnitude < 0.05f)
                {
                    insideForce = Vector3.zero;
                }
            }
            else
            {
                insideForce = Vector3.zero;
            }


            Vector2 forceDirection = new Vector2(angle.x, angle.y).normalized;
            var additionalForce = forceDirection * (_addedForce * Time.deltaTime);
            var updatedDirection = (forceDirection * rigidbody2D.linearVelocity.magnitude +
                                    (Vector2)insideForce.normalized * (Time.deltaTime * _rotationSpeed)).normalized;
            rigidbody2D.linearVelocity = updatedDirection * (rigidbody2D.linearVelocity.magnitude) + additionalForce;
        }

        private void OnDrawGizmos()
        {
            //debug draw closest point 

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_closestPoint, 0.2f);
            var angle = _splineContainer.Spline.EvaluateTangent(_closestPointT);
            Gizmos.DrawLine(_closestPoint, _closestPoint + 0.5f * new Vector2(angle.x, angle.y));
        }

        private void Awake()
        {
            _playerMovement = FindObjectOfType<PlayerMovement>();
            _playerMovement.DragFinished += PlayerMovement_DragFinished;
        }

        private void OnDestroy()
        {
            _playerMovement.DragFinished -= PlayerMovement_DragFinished;
        }
    }
}
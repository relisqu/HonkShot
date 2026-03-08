using System.Collections.Generic;
using Scripts.LevelSystem.LevelObjects;
using Scripts.Player.InputHandling;
using UnityEngine;

namespace Scripts.Player
{
    public class RaycastTrajectory : MonoBehaviour
    {
        [Header("Visuals")] [SerializeField] private LineRenderer _line;
        [SerializeField] private Vector3 _lineOffset;

        [Header("Settings")] [SerializeField] private int _maxBounces = 2;
        [SerializeField] private float _maxSegmentDistance = 50f;
        [SerializeField] private LayerMask _bounceLayerMask;
        [SerializeField] private CircleCollider2D _ballCollider;
        [SerializeField] private float _bounceOriginOffset = 0.05f;

        [Header("Debug")] [SerializeField] private bool _debugMode;

        [Header("References")] [SerializeField]
        private InputHandler _inputHandler;

        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerBallMovement _playerBallMovement;

        private bool _isDragging;
        private Vector2 _cachedDrag;
        [SerializeField] private float _dragChangeThreshold = 0.01f;

        private struct BounceDebugData
        {
            public Vector2 ContactPoint;
            public Vector2 Normal;
            public Vector2 Incoming;
            public Vector2 Reflected;
            public float Radius;
            public Vector2 BallCenter;
            public float OtherRadius;
            public Vector2 OtherCenter;
            public bool OtherIsCircle;
        }

        private readonly List<Vector3> _linePoints = new();

        private readonly List<BounceDebugData> _debugBounces = new();
        private readonly List<Vector3> _debugTrajectoryPoints = new();
        private Vector3 _debugStartPos;
        private Vector2 _debugDrag;

        private ContactFilter2D _contactFilter;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

        private void OnEnable()
        {
            _playerMovement.DragStarted += PlayerMovement_DragStarted;
            _playerMovement.DragFinished += PlayerMovement_DragFinished;
            _playerMovement.InputCancelled += PlayerMovement_InputCancelled;

            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(_bounceLayerMask);
            _contactFilter.useTriggers = false;
        }

        private void OnDisable()
        {
            _playerMovement.DragStarted -= PlayerMovement_DragStarted;
            _playerMovement.DragFinished -= PlayerMovement_DragFinished;
            _playerMovement.InputCancelled -= PlayerMovement_InputCancelled;
        }

        private void PlayerMovement_DragStarted()
        {
            _isDragging = true;
            _line.positionCount = 0;
        }

        private void PlayerMovement_DragFinished(Vector2 _)
        {
            _isDragging = false;
            if (!_debugMode)
                _line.positionCount = 0;
        }

        private void PlayerMovement_InputCancelled()
        {
            _isDragging = false;
            _line.positionCount = 0;
        }

        private void Update()
        {
            if (!_isDragging) return;

            Vector2 currentDrag = -_inputHandler.GetCurrentDrag();
            if (currentDrag.sqrMagnitude < _dragChangeThreshold * _dragChangeThreshold)
            {
                _line.positionCount = 0;
                return;
            }

            _cachedDrag = currentDrag;
            CalculateTrajectory(_cachedDrag);
        }

        private void CalculateTrajectory(Vector2 drag)
        {
            _linePoints.Clear();
            _debugBounces.Clear();
            _debugTrajectoryPoints.Clear();

            float radius = _ballCollider.radius * Mathf.Abs(_ballCollider.transform.lossyScale.x);
            Vector2 startPos = _ballCollider.transform.TransformPoint(_ballCollider.offset);
            _debugStartPos = startPos;
            _debugDrag = drag;

            float throwSpeed = _playerBallMovement.GetDragVelocityMagnitude(_playerMovement.GetRigidbody(), drag);
            Vector2 direction = drag;

            _linePoints.Add((Vector3)startPos);
            _debugTrajectoryPoints.Add((Vector3)startPos);

            Vector2 origin = startPos;
            Collider2D lastHitCollider = null;

            int maxBounces = _maxBounces;
            for (int bounce = 0; bounce <= maxBounces; bounce++)
            {
                int hitCount = Physics2D.CircleCast(
                    origin, radius, direction, _contactFilter, _hitBuffer, _maxSegmentDistance);

                RaycastHit2D hit = default;
                for (int i = 0; i < hitCount; i++)
                {
                    if (_hitBuffer[i].distance <= 0f) continue;
                    if (_hitBuffer[i].collider == lastHitCollider && _hitBuffer[i].distance < 0.01f) continue;
                    if (!hit.collider || _hitBuffer[i].distance < hit.distance)
                        hit = _hitBuffer[i];
                }

                if (hit.collider)
                {
                    lastHitCollider = hit.collider;
                    _linePoints.Add((Vector3)hit.centroid);
                    _debugTrajectoryPoints.Add((Vector3)hit.centroid);

                    float bounciness = 1f;
                    if (hit.collider.TryGetComponent(out BounceObject bounceObj))
                        bounciness = bounceObj.Bounciness;
                    else if (hit.transform.parent
                             && hit.transform.parent.TryGetComponent(out bounceObj))
                        bounciness = bounceObj.Bounciness;

                    Vector2 reflected = Vector2.Reflect(direction, hit.normal).normalized;

                    Vector2 otherCenter = Vector2.zero;

                    _debugBounces.Add(new BounceDebugData
                    {
                        ContactPoint = hit.point,
                        Normal = hit.normal,
                        Incoming = direction,
                        Reflected = reflected,
                        Radius = radius,
                        BallCenter = hit.centroid,
                        OtherCenter = otherCenter,
                    });

                    direction = reflected;
                    origin = hit.centroid + reflected * _bounceOriginOffset;
                    throwSpeed *= bounciness;
                }
                else
                {
                    var endPoint = (Vector3)(origin + direction * _maxSegmentDistance);
                    _linePoints.Add(endPoint);
                    _debugTrajectoryPoints.Add(endPoint);
                    break;
                }
            }

            _line.positionCount = _linePoints.Count;
            _line.SetPositions(_linePoints.ToArray());

            if (_debugMode)
            {
                var sb = new System.Text.StringBuilder($"Trajectory: {_linePoints.Count} pts, dir={direction} | ");
                for (int i = 0; i < _linePoints.Count; i++)
                    sb.Append($"[{i}]={_linePoints[i]:F2} ");
                Debug.Log(sb.ToString());
            }
        }

        private void OnDrawGizmos()
        {
            if (!_debugMode) return;

            if (_debugDrag.sqrMagnitude > 0.001f)
            {
                Vector3 dragEnd = _debugStartPos + (Vector3)_debugDrag;
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(_debugStartPos, dragEnd);
                Gizmos.DrawSphere(dragEnd, 0.06f);
            }

            for (int i = 0; i < _debugTrajectoryPoints.Count; i++)
            {
                bool isStart = i == 0;
                bool isEnd = i == _debugTrajectoryPoints.Count - 1;

                Gizmos.color = isStart ? Color.green : isEnd ? Color.red : Color.yellow;
                Gizmos.DrawSphere(_debugTrajectoryPoints[i], isStart ? 0.12f : 0.08f);

                if (i > 0)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(_debugTrajectoryPoints[i - 1], _debugTrajectoryPoints[i]);
                }
            }

            for (int i = 0; i < _debugBounces.Count; i++)
            {
                var b = _debugBounces[i];
                Vector3 point = b.BallCenter;

                Gizmos.color = new Color(0.3f, 0.3f, 1f);
                Gizmos.DrawLine(point, point + (Vector3)(b.Normal * 2f));
                Gizmos.DrawCube(point + (Vector3)(b.Normal * 2f), Vector3.one * 0.1f);

                Gizmos.color = new Color(0.5f, 0.5f, 1f);
                Gizmos.DrawLine(point, point + (Vector3)(b.Incoming * 2f));

                Gizmos.color = new Color(0f, 0.5f, 1f);
                Gizmos.DrawLine(point, point + (Vector3)(b.Reflected * 2f));
                Gizmos.DrawSphere(point + (Vector3)(b.Reflected * 2f), 0.05f);

                Gizmos.color = new Color(0.3f, 0.3f, 1f, 0.3f);
                DrawGizmoCircle((Vector3)b.BallCenter, b.Radius, 24);

                if (b.OtherIsCircle && b.OtherRadius > 0)
                {
                    Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
                    DrawGizmoCircle((Vector3)b.OtherCenter, b.OtherRadius, 24);
                }
            }
        }

        private static void DrawGizmoCircle(Vector3 center, float radius, int segments)
        {
            float step = 360f / segments;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = step * i * Mathf.Deg2Rad;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}
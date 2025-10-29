using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace Scripts.LevelSystem.LevelObjects
{
    [ExecuteInEditMode]
    public class WallGeneration : MonoBehaviour
    {
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private Transform _wallsContainer;
        [SerializeField] private SpriteShapeController _levelField;
        [SerializeField] private SpriteShapeController _backgroundLevelField;
        [SerializeField] private Vector3 _fieldOffset;

        private void Start()
        {
            //GenerateWalls();
            CopySpriteShapeSpline();
            
        }

        private int nextUpdate = 1;

        private void Update()
        {
            if (Time.time < nextUpdate) return;

            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            GenerateWalls();
        }

        [Button]
        public void CopySpriteShapeSpline()
        {
            var srcSpline = _levelField.spline;
            var dstSpline = _backgroundLevelField.spline;
            _backgroundLevelField.transform.position = _levelField.transform.position + _fieldOffset;

            // convenience
            var srcTr = _levelField.transform;

            // wipe dst + recreate positions
            dstSpline.Clear();

            int pointCount = srcSpline.GetPointCount();
            bool isOpen = srcSpline.isOpenEnded;

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 pos = srcSpline.GetPosition(i);
                dstSpline.InsertPointAt(i, pos);
            }

            // now copy per-point data + set adaptive height
            for (int i = 0; i < pointCount; i++)
            {
                // --- basic copy ---
                Vector3 pos = srcSpline.GetPosition(i);
                dstSpline.SetPosition(i, pos);

                Vector3 leftTangent = srcSpline.GetLeftTangent(i);
                Vector3 rightTangent = srcSpline.GetRightTangent(i);
                dstSpline.SetLeftTangent(i, leftTangent);
                dstSpline.SetRightTangent(i, rightTangent);

                var mode = srcSpline.GetTangentMode(i);
                dstSpline.SetTangentMode(i, mode);

                // --- decide top vs bottom for this corner ---
                // pick prev / next indices
                int prevI, nextI;
                if (isOpen)
                {
                    // clamp ends if open
                    prevI = Mathf.Max(i - 1, 0);
                    nextI = Mathf.Min(i + 1, pointCount - 1);
                }
                else
                {
                    // wrap if closed
                    prevI = (i - 1 + pointCount) % pointCount;
                    nextI = (i + 1) % pointCount;
                }

                Vector2 prevPosL = srcSpline.GetPosition(prevI);
                Vector2 currPosL = srcSpline.GetPosition(i);
                Vector2 nextPosL = srcSpline.GetPosition(nextI);

                Vector2 dirPrevL = (currPosL - prevPosL).normalized;
                Vector2 dirNextL = (nextPosL - currPosL).normalized;

                // handle degenerate straight points safely
                if (dirPrevL.sqrMagnitude < 1e-6f || dirNextL.sqrMagnitude < 1e-6f)
                {
                    // fallback: just use source height scaled
                    float baseH = srcSpline.GetHeight(i);
                    dstSpline.SetHeight(i, baseH * 1.5f);
                    continue;
                }

                // outward-ish normals for each edge in LOCAL space
                // (-y, x) = rotate 90° CCW
                Vector2 n1L = new Vector2(-dirPrevL.y, dirPrevL.x);
                Vector2 n2L = new Vector2(-dirNextL.y, dirNextL.x);

                // average them -> corner normal (LOCAL)
                Vector2 avgNormalL = n1L + n2L;
                float mag = avgNormalL.magnitude;
                if (mag > 1e-6f)
                    avgNormalL /= mag;
                else
                {
                    // fallback if basically straight
                    float baseH = srcSpline.GetHeight(i);
                    dstSpline.SetHeight(i, baseH * 1.5f);
                    continue;
                }

                // convert that averaged normal to WORLD space so we know "up" vs "down"
                Vector2 avgNormalW = ((Vector2)srcTr.TransformDirection(
                    new Vector3(avgNormalL.x, avgNormalL.y, 0f)
                )).normalized;

                // how much it faces up vs down
                float dotUp = Vector2.Dot(avgNormalW, Vector2.up); // 1 = perfectly up
                float dotDown = Vector2.Dot(avgNormalW, Vector2.down); // 1 = perfectly down

                bool isTopEdge = dotUp >= dotDown && dotUp > 0f;
                // if it's leaning more up than down, we call it "top"

                // choose thickness
                float thickness = isTopEdge ? 0.1f : 0.5f;

                // assign
                dstSpline.SetHeight(i, thickness);
            }

            // mirror open/closed setting
            dstSpline.isOpenEnded = srcSpline.isOpenEnded;

            // finally refresh target visuals/collider
            _backgroundLevelField.RefreshSpriteShape();
        }

        [Button]
        public void GenerateWalls()
        {
            Spline levelSprite = _levelField.spline;

            for (int i = 0; i < levelSprite.GetPointCount(); i++)
            {
                // Check if point is a corner
                if (levelSprite.GetCorner(i))
                {
                    levelSprite.SetHeight(i, 0.1f);
                }
            }

            // Update the SpriteShape after modifications
            _levelField.RefreshSpriteShape();
            if (_wallPrefab == null || _wallsContainer == null || _levelField == null)
            {
                Debug.LogError("Please assign all required references.");
                return;
            }

            ClearContainer();
            Spline spline = _levelField.spline;

            for (int i = 0; i < spline.GetPointCount(); i++)
            {
                Vector3 currentPoint = spline.GetPosition(i); // Get the position of the current control point
                Vector3 nextPoint =
                    spline.GetPosition((i + 1) %
                                       spline
                                           .GetPointCount()); // Get the next control point (looping back to the first)

                Vector3 worldCurrentPoint = _levelField.transform.TransformPoint(currentPoint);
                Vector3 worldNextPoint = _levelField.transform.TransformPoint(nextPoint);
                Vector3 midpoint = (worldCurrentPoint + worldNextPoint) / 2f;

                Vector3 direction = worldNextPoint - worldCurrentPoint;
                float rotationZ = 90 + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                GameObject wall = Instantiate(_wallPrefab, midpoint, Quaternion.Euler(0, 0, rotationZ),
                    _wallsContainer);

                float wallLength = direction.magnitude;
                wall.transform.localScale =
                    new Vector3(wall.transform.localScale.x, wallLength + 0.2f, 1);
            }
        }

        private void ClearContainer()
        {
            for (int i = _wallsContainer.childCount; i > 0; --i)
                DestroyImmediate(_wallsContainer.GetChild(0).gameObject);
        }
    }
}
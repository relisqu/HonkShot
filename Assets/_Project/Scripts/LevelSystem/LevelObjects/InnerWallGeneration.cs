using System.Collections.Generic;
using Scripts.LevelSystem.LevelGeneration;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace Scripts.LevelSystem.LevelObjects
{
    [ExecuteAlways]
    public class InnerWallGeneration : MonoBehaviour
    {
        [Header("Source Shapes")]
        [SerializeField] private SpriteShapeController _innerShape;
        [SerializeField] private SpriteShapeController _helperShape;
        [Tooltip("Optional reference to the outer playable field so trees never leak outside it.")]
        [SerializeField] private SpriteShapeController _outerLevelField;

        [Header("Walls")]
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private Transform _wallsContainer;
        [SerializeField] private float _cornerThickness = 0.1f;
        [SerializeField] private float _wallLengthPadding = 0.2f;
        [Tooltip("Width (X scale) multiplier applied to walls whose outward normal points upward.")]
        [SerializeField, Min(0f)] private float _topWallThicknessMultiplier = 1.5f;
        [Tooltip("World-space distance to push top walls toward the hole's center (fakes depth receding).")]
        [SerializeField, Min(0f)] private float _topWallInwardOffset = 0.15f;
        [Tooltip("An edge counts as 'top' when its world outward normal has dot(up) >= this threshold.")]
        [SerializeField, Range(-1f, 1f)] private float _topWallDotUpThreshold = 0.25f;

        [Header("Helper Depth Shape")]
        [Tooltip("World-space Y offset applied to TOP spline points of the helper shape (faked depth rim).")]
        [SerializeField] private float _topPointHeightOffset = 0.2f;
        [SerializeField] private float _helperHeightTop = 0.1f;
        [SerializeField] private float _helperHeightBottom = 0.5f;

        [Header("Floor Color Binding")]
        [SerializeField] private bool _bindFloorColor = true;
        [Tooltip("Default color applied to inner shape before a floor event fires.")]
        [SerializeField] private Color _defaultInnerColor = Color.black;

        [Header("Trees")]
        [SerializeField] private Transform _treesContainer;
        [Tooltip("Shared decoration pool (same SO used by BackgroundPropSpawner). PropDefinitions are sampled as trees inside the hole.")]
        [SerializeField] private FloorDecorationsSO _floorDecorations;
        [SerializeField, Min(0)] private int _treeSpawnAttempts = 40;
        [SerializeField, Min(0)] private int _maxTreeCount = 8;
        [SerializeField, Min(0f)] private float _treeMinSeparation = 0.8f;
        [SerializeField, Min(0f)] private float _treeEdgePadding = 0.3f;
        [SerializeField] private int _randomSeed = 0;
        [SerializeField] private string _treeSortingLayerName = "Default";
        [SerializeField] private int _treeSortingOrder = 5;

        [Header("Auto Update")]
        [Tooltip("When true, rebuilds walls/helper/trees whenever the inner spline changes in the editor.")]
        [SerializeField] private bool _autoRefreshInEditor = true;

        [Tooltip("When true, the wall is locked: it will NOT auto-rebuild on Start, on editor spline changes, or on floor enter. Manual [Button] calls still work.")]
        [SerializeField] private bool _locked = false;

        private int _cachedSplineHash;
        private SpriteShapeRenderer _innerRenderer;

        private void Start()
        {
            if (!Application.isPlaying) return;

            if (!_locked)
                RebuildAll();

            if (_bindFloorColor)
                ApplyFloorColor(_defaultInnerColor);

            if (LevelManager.Instance)
                LevelManager.Instance.FloorEntered += LevelManager_FloorEntered;
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.FloorEntered -= LevelManager_FloorEntered;
        }

        public void SetFloorDecorations(FloorDecorationsSO decorations)
        {
            _floorDecorations = decorations;
        }

        private void Update()
        {
            if (Application.isPlaying) return;
            if (_locked) return;
            if (!_autoRefreshInEditor) return;
            if (!_innerShape) return;

            int currentHash = ComputeSplineHash(_innerShape.spline);
            if (currentHash != _cachedSplineHash)
            {
                _cachedSplineHash = currentHash;
                RebuildAll();
            }
        }

        private void LevelManager_FloorEntered(FloorConfigSO floorConfig)
        {
            if (!floorConfig) return;

            if (_bindFloorColor)
                ApplyFloorColor(floorConfig.BackgroundColor);

            if (_locked) return;

            if (floorConfig.Decorations)
            {
                _floorDecorations = floorConfig.Decorations;
                GenerateTrees();
            }
        }

        private void ApplyFloorColor(Color color)
        {
            if (!_innerShape) return;
            if (!_innerRenderer)
                _innerRenderer = _innerShape.GetComponent<SpriteShapeRenderer>();
            if (_innerRenderer)
                _innerRenderer.color = color;
        }

        [Button]
        public void RebuildAll()
        {
            CopySpriteShapeSplineToHelper();
            GenerateWalls();
            GenerateTrees();
        }

        [Button]
        public void CopySpriteShapeSplineToHelper()
        {
            if (!_innerShape || !_helperShape) return;

            var src = _innerShape.spline;
            var dst = _helperShape.spline;

            _helperShape.transform.position = _innerShape.transform.position;
            _helperShape.transform.rotation = _innerShape.transform.rotation;
            _helperShape.transform.localScale = _innerShape.transform.localScale;

            dst.Clear();

            int count = src.GetPointCount();
            bool isOpen = src.isOpenEnded;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = src.GetPosition(i);
                dst.InsertPointAt(i, pos);
            }

            var srcTr = _innerShape.transform;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = src.GetPosition(i);
                Vector3 leftTangent = src.GetLeftTangent(i);
                Vector3 rightTangent = src.GetRightTangent(i);
                var mode = src.GetTangentMode(i);

                int prevI, nextI;
                if (isOpen)
                {
                    prevI = Mathf.Max(i - 1, 0);
                    nextI = Mathf.Min(i + 1, count - 1);
                }
                else
                {
                    prevI = (i - 1 + count) % count;
                    nextI = (i + 1) % count;
                }

                Vector2 prevPosL = src.GetPosition(prevI);
                Vector2 currPosL = src.GetPosition(i);
                Vector2 nextPosL = src.GetPosition(nextI);

                Vector2 dirPrevL = (currPosL - prevPosL).normalized;
                Vector2 dirNextL = (nextPosL - currPosL).normalized;

                bool isTopEdge = false;

                if (dirPrevL.sqrMagnitude > 1e-6f && dirNextL.sqrMagnitude > 1e-6f)
                {
                    Vector2 n1L = new Vector2(-dirPrevL.y, dirPrevL.x);
                    Vector2 n2L = new Vector2(-dirNextL.y, dirNextL.x);

                    Vector2 avgNormalL = n1L + n2L;
                    if (avgNormalL.sqrMagnitude > 1e-6f)
                    {
                        avgNormalL.Normalize();
                        Vector2 avgNormalW = ((Vector2)srcTr.TransformDirection(
                            new Vector3(avgNormalL.x, avgNormalL.y, 0f)
                        )).normalized;

                        float dotUp = Vector2.Dot(avgNormalW, Vector2.up);
                        float dotDown = Vector2.Dot(avgNormalW, Vector2.down);
                        isTopEdge = dotUp >= dotDown && dotUp > 0f;
                    }
                }

                if (isTopEdge)
                    pos.y += _topPointHeightOffset;

                dst.SetPosition(i, pos);
                dst.SetLeftTangent(i, leftTangent);
                dst.SetRightTangent(i, rightTangent);
                dst.SetTangentMode(i, mode);
                dst.SetHeight(i, isTopEdge ? _helperHeightTop : _helperHeightBottom);
            }

            dst.isOpenEnded = src.isOpenEnded;
            _helperShape.RefreshSpriteShape();
        }

        [Button]
        public void GenerateWalls()
        {
            if (!_innerShape || !_wallPrefab || !_wallsContainer) return;

            var spline = _innerShape.spline;

            for (int i = 0; i < spline.GetPointCount(); i++)
            {
                if (spline.GetCorner(i))
                    spline.SetHeight(i, _cornerThickness);
            }

            _innerShape.RefreshSpriteShape();

            ClearContainer(_wallsContainer);

            int count = spline.GetPointCount();

            Vector3 polygonCenterW = Vector3.zero;
            for (int i = 0; i < count; i++)
                polygonCenterW += _innerShape.transform.TransformPoint(spline.GetPosition(i));
            polygonCenterW /= Mathf.Max(1, count);

            for (int i = 0; i < count; i++)
            {
                Vector3 currentL = spline.GetPosition(i);
                Vector3 nextL = spline.GetPosition((i + 1) % count);

                Vector3 currentW = _innerShape.transform.TransformPoint(currentL);
                Vector3 nextW = _innerShape.transform.TransformPoint(nextL);
                Vector3 midpoint = (currentW + nextW) * 0.5f;

                Vector3 direction = nextW - currentW;
                float wallLength = direction.magnitude;
                float rotationZ = 90f + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Decide if this edge is the "top" of the hole (outward normal faces up in world space).
                Vector2 dirN = ((Vector2)direction).normalized;
                Vector2 outwardNormalW = new Vector2(-dirN.y, dirN.x);
                Vector2 midToCenter = ((Vector2)polygonCenterW - (Vector2)midpoint).normalized;

                // Flip the outward normal if it's pointing INTO the polygon (handles CW/CCW winding).
                if (Vector2.Dot(outwardNormalW, midToCenter) > 0f)
                    outwardNormalW = -outwardNormalW;

                float dotUp = Vector2.Dot(outwardNormalW, Vector2.up);
                bool isTopEdge = dotUp >= _topWallDotUpThreshold;

                Vector3 spawnPos = midpoint;
                if (isTopEdge)
                    spawnPos += (Vector3)(midToCenter * _topWallInwardOffset);

                GameObject wall = Instantiate(
                    _wallPrefab,
                    spawnPos,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    _wallsContainer
                );

                Vector3 baseScale = wall.transform.localScale;
                float widthScale = isTopEdge ? baseScale.x * _topWallThicknessMultiplier : baseScale.x;
                wall.transform.localScale = new Vector3(
                    widthScale,
                    wallLength + _wallLengthPadding,
                    1f
                );
            }
        }

        [Button]
        public void GenerateTrees()
        {
            if (!_treesContainer) return;

            ClearContainer(_treesContainer);

            if (!_innerShape) return;
            if (!_floorDecorations || _floorDecorations.PropDefinitions == null || _floorDecorations.PropDefinitions.Count == 0) return;
            if (_maxTreeCount <= 0) return;

            var innerPoly = BuildWorldPolygon(_innerShape);
            if (innerPoly.Count < 3) return;

            var outerPoly = _outerLevelField ? BuildWorldPolygon(_outerLevelField) : null;
            Bounds innerBounds = ComputePolygonBounds(innerPoly);

            Random.State savedState = default;
            bool seedOverridden = _randomSeed != 0;
            if (seedOverridden)
            {
                savedState = Random.state;
                Random.InitState(_randomSeed);
            }

            var placedCenters = new List<Vector2>();
            var pool = _floorDecorations.PropDefinitions;
            int spawned = 0;

            for (int a = 0; a < _treeSpawnAttempts && spawned < _maxTreeCount; a++)
            {
                Vector2 candidate = new Vector2(
                    Random.Range(innerBounds.min.x, innerBounds.max.x),
                    Random.Range(innerBounds.min.y, innerBounds.max.y)
                );

                if (!IsPointInsidePolygon(candidate, innerPoly))
                    continue;

                FloorDecorationsSO.PropDefinition def = pool[Random.Range(0, pool.Count)];
                if (def == null || def.sprites == null || def.sprites.Length == 0) continue;

                Sprite sprite = def.sprites[Random.Range(0, def.sprites.Length)];
                if (!sprite) continue;

                float scale = Random.Range(def.randomScaleRange.x, def.randomScaleRange.y);

                Vector2 jitter = new Vector2(
                    Random.Range(-def.positionJitterRange.x, def.positionJitterRange.x),
                    Random.Range(-def.positionJitterRange.y, def.positionJitterRange.y)
                );
                Vector2 jittered = candidate + jitter;

                if (!IsPointInsidePolygon(jittered, innerPoly))
                    continue;

                Vector2 half = sprite.bounds.extents * scale;
                Vector2 padded = new Vector2(half.x + _treeEdgePadding, half.y + _treeEdgePadding);

                if (!RectInsidePolygon(jittered, padded, innerPoly))
                    continue;

                if (outerPoly != null && outerPoly.Count >= 3 && !IsPointInsidePolygon(jittered, outerPoly))
                    continue;

                bool tooClose = false;
                float sepSq = _treeMinSeparation * _treeMinSeparation;
                for (int k = 0; k < placedCenters.Count; k++)
                {
                    if ((placedCenters[k] - jittered).sqrMagnitude < sepSq)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                SpawnTree(sprite, jittered, scale);
                placedCenters.Add(jittered);
                spawned++;
            }

            if (seedOverridden)
                Random.state = savedState;
        }

        private void SpawnTree(Sprite sprite, Vector2 worldPos, float scale)
        {
            var go = new GameObject("_innerTree");
            go.transform.SetParent(_treesContainer, worldPositionStays: false);
            go.transform.position = worldPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = _treeSortingLayerName;
            sr.sortingOrder = _treeSortingOrder;

            go.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static List<Vector2> BuildWorldPolygon(SpriteShapeController shape)
        {
            var list = new List<Vector2>();
            var spline = shape.spline;
            int count = spline.GetPointCount();
            if (spline.isOpenEnded || count < 3) return list;

            for (int i = 0; i < count; i++)
                list.Add(shape.transform.TransformPoint(spline.GetPosition(i)));

            return list;
        }

        private static Bounds ComputePolygonBounds(List<Vector2> poly)
        {
            if (poly.Count == 0) return new Bounds();

            var b = new Bounds(poly[0], Vector3.zero);
            for (int i = 1; i < poly.Count; i++)
                b.Encapsulate((Vector3)poly[i]);
            return b;
        }

        private static bool IsPointInsidePolygon(Vector2 p, List<Vector2> poly)
        {
            bool inside = false;
            int count = poly.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Vector2 pi = poly[i];
                Vector2 pj = poly[j];
                bool intersect = (pi.y > p.y) != (pj.y > p.y) &&
                                 p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y + 1e-7f) + pi.x;
                if (intersect) inside = !inside;
            }
            return inside;
        }

        private static bool RectInsidePolygon(Vector2 center, Vector2 half, List<Vector2> poly)
        {
            Vector2 c00 = new Vector2(center.x - half.x, center.y - half.y);
            Vector2 c10 = new Vector2(center.x + half.x, center.y - half.y);
            Vector2 c01 = new Vector2(center.x - half.x, center.y + half.y);
            Vector2 c11 = new Vector2(center.x + half.x, center.y + half.y);
            return IsPointInsidePolygon(c00, poly)
                && IsPointInsidePolygon(c10, poly)
                && IsPointInsidePolygon(c01, poly)
                && IsPointInsidePolygon(c11, poly);
        }

        private static int ComputeSplineHash(Spline spline)
        {
            int count = spline.GetPointCount();
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + count;
                for (int i = 0; i < count; i++)
                {
                    Vector3 p = spline.GetPosition(i);
                    hash = hash * 31 + p.x.GetHashCode();
                    hash = hash * 31 + p.y.GetHashCode();
                }
                return hash;
            }
        }

        private void ClearContainer(Transform container)
        {
            if (!container) return;

            for (int i = container.childCount; i > 0; --i)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(container.GetChild(0).gameObject);
                else
#endif
                    Destroy(container.GetChild(0).gameObject);
            }
        }
    }
}

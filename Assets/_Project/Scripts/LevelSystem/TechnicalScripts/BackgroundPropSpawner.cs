using Scripts.LevelSystem.LevelGeneration;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D; // SpriteShapeController
using PropDefinition = Scripts.LevelSystem.LevelGeneration.FloorDecorationsSO.PropDefinition;

namespace Scripts.LevelSystem.TechnicalScripts
{
    /// <summary>
    /// Hybrid spawner:
    /// 1. Spawns props around the level spline border.
    /// 2. ALSO spawns props near the camera's screen corners for framing.
    ///
    /// Extras:
    /// - Global anti-clump radius (_minSeparation).
    /// - Parallax-ish scaling (bigger/front at bottom edges, smaller/back at top edges).
    /// - Shared material instance for batching.
    /// - Per-instance MaterialPropertyBlock for unique _DistanceFactor / _WindFactor.
    /// - CULLING STEP: after spawn we sample 3 points on the sprite
    ///     * top    (center.x, bounds.max.y)
    ///     * middle (center.x, bounds.center.y)
    ///     * bottom (center.x, bounds.min.y)
    ///   If ANY of those points is inside the playable field polygon,
    ///   that prop is destroyed (so nothing visually sits on top of the field).
    /// </summary>
    [ExecuteAlways]
    public class BackgroundPropSpawner : MonoBehaviour
    {

        // -------------------------------------------------
        // References
        // -------------------------------------------------
        [Header("References")]
        [Tooltip("The SpriteShape that defines the playable field boundary.")]
        [SerializeField] private SpriteShapeController _levelField;

        [Tooltip("All spawned background props will be created under this transform.")]
        [SerializeField] private Transform _spawnParent;

        [Tooltip("Camera that defines the visible screen for corner spawns. If null we try Camera.main.")]
        [SerializeField] private UnityEngine.Camera _camera;

        // -------------------------------------------------
        // Spline border spawn settings
        // -------------------------------------------------
        [Header("Spline Border Placement")]
        [Tooltip("Distance (world units) OUTWARD from the level border to place props (min/max).")]
        [SerializeField] private Vector2 _spawnDistanceRange = new Vector2(0.4f, 1.0f);

        [Tooltip("Approx spacing along each spline edge in world units.")]
        [SerializeField, Min(0.05f)] private float _spacing = 1.5f;

        [Tooltip("Global Y offset to push every prop upward (visual ground alignment).")]
        public float _yOffset = 1.5f;

        [Tooltip("Don't bother spawning along very tiny edges.")]
        [SerializeField, Min(0f)] private float _minSegmentLengthForSpawn = 0.3f;

        [Tooltip("Safety cap: max props we allow on a single spline segment.")]
        [SerializeField, Min(1)] private int _maxPerSegment = 10;

        [Tooltip("Chance (0..1) that we actually spawn at each candidate point on spline.")]
        [SerializeField, Range(0f, 1f)] private float _spawnChance = 0.8f;

        // -------------------------------------------------
        // De-clumping
        // -------------------------------------------------
        [Header("De-clump / Density Control")]
        [Tooltip("Minimal world distance between ANY two spawned props. Bigger = less cramped.")]
        [SerializeField, Min(0f)] private float _minSeparation = 1.0f;

        [SerializeField, Min(0f)] private float _blurForce = 1.0f;

        // Global list for anti-clump, filled per GenerateProps() call
        private readonly List<Vector2> _usedPositions = new List<Vector2>();

        // -------------------------------------------------
        // Screen corner spawn settings
        // -------------------------------------------------
        [Header("Screen Corner Spawning")]
        [Tooltip("Also sprinkle props near the 4 camera screen corners.")]
        [SerializeField] private bool _spawnScreenCorners = true;

        [Tooltip("How many props to TRY per corner.")]
        [SerializeField, Min(0)] private int _cornerPropsPerCorner = 3;

        [Tooltip("Radius (world units) inward from each corner to scatter props.")]
        [SerializeField, Min(0f)] private float _cornerRadius = 2f;

        // -------------------------------------------------
        // Props library
        // -------------------------------------------------
        [Header("Props Library")]
        [Tooltip("Shared per-floor decoration pool. When assigned at runtime, its PropDefinitions override _propDefinitions.")]
        [SerializeField] private FloorDecorationsSO _floorDecorations;

        [Tooltip("Inline fallback prop definitions used when _floorDecorations is null.")]
        [SerializeField] private List<PropDefinition> _propDefinitions = new List<PropDefinition>();

        // -------------------------------------------------
        // Sorting / Rendering
        // -------------------------------------------------
        [Header("Sorting / Rendering")]
        [Tooltip("Sorting layer for all spawned props.")]
        [SerializeField] private string _sortingLayerName = "Background";

        [Tooltip("Sorting order for props that are BEHIND the level (usually top/back side).")]
        [SerializeField] private int _backSortingOrder = -10;

        [Tooltip("Sorting order for props that are IN FRONT of the level (usually bottom/front side).")]
        [SerializeField] private int _frontSortingOrder = 10;

        [Tooltip("Edge is 'front' if its outward normal points downward at least this much.\n" +
                 "0 => most downward-ish edges are 'front'. 1 => only perfectly down is 'front'.")]
        [SerializeField, Range(-1f, 1f)] private float _frontDotDownThreshold = 0f;

        // -------------------------------------------------
        // Material / Shader overrides
        // -------------------------------------------------
        [Header("Material / Shader Overrides")]
        [Tooltip("Source material used by all spawned props.\nWe clone it ONCE into a shared runtime material for batching.")]
        [SerializeField] private Material _propMaterial;

        [Tooltip("Float property: recolor factor based on distance from border. 0 near, 1 far.")]
        [SerializeField] private string _distanceShaderFloatName = "_DistanceFactor";

        [Tooltip("Float property: per-instance wind sway factor / phase.")]
        [SerializeField] private string _windShaderFloatName = "_WindFactor";

        [Tooltip("Random range for wind factor per spawned prop.")]
        [SerializeField] private Vector2 _windRandomRange = new Vector2(0f, 1f);

        // Single shared material instance for all spawned props (for batching)
        private Material _sharedRuntimeMaterial;

        // -------------------------------------------------
        // Parallax / depth feeling
        // -------------------------------------------------
        [Header("Parallax / Depth Illusion")]
        [Tooltip("Scale at TOP side of map/screen (dotDown ~ -1).")]
        [SerializeField] private float _topScale = 0.9f;

        [Tooltip("Scale at BOTTOM side of map/screen (dotDown ~ +1).")]
        [SerializeField] private float _bottomScale = 1.2f;

        // -------------------------------------------------
        // Runtime / debug
        // -------------------------------------------------
        [Header("Runtime")]
        [Tooltip("If true, auto-generate in Awake.")]
        [SerializeField] private bool _autoGenerateOnAwake = false;

        [Tooltip("Extra debug logging.")]
        [SerializeField] private bool _debugLogs = false;

        private const string CHILD_TAG = "_bgProp";

        // Cached world-space polygon of the level for inside tests
        // (used for culling props that overlap level visually)
        private readonly List<Vector2> _cachedLevelPolygon = new List<Vector2>();
        private bool _cachedPolygonValid = false;

        private void Awake()
        {
            if (_autoGenerateOnAwake)
                GenerateProps();
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            if (LevelManager.Instance)
                LevelManager.Instance.FloorEntered += LevelManager_FloorEntered;
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.FloorEntered -= LevelManager_FloorEntered;
        }

        private void LevelManager_FloorEntered(Scripts.LevelSystem.LevelGeneration.FloorConfigSO floorConfig)
        {
            if (!floorConfig || !floorConfig.Decorations) return;
            _floorDecorations = floorConfig.Decorations;
            if (_levelField)
                GenerateProps();
        }

        public void SetFloorDecorations(FloorDecorationsSO decorations)
        {
            _floorDecorations = decorations;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_levelField == null)
                _levelField = GetComponent<SpriteShapeController>();
        }
#endif

        public void SetLevel(SpriteShapeController spriteField)
        {
            _levelField = spriteField;
            GenerateProps();
        }

        private List<PropDefinition> ActivePropDefinitions => _floorDecorations ? _floorDecorations.PropDefinitions : _propDefinitions;

        [Button]
        [ContextMenu("Generate Background Props")]
        public void GenerateProps()
        {
            if (_levelField == null)
            {
                Debug.LogWarning("[BackgroundPropSpawner] _levelField is null (need spline for border spawn).");
                return;
            }

            if (_spawnParent == null)
            {
                Debug.LogWarning("[BackgroundPropSpawner] _spawnParent is null");
                return;
            }

            var activeDefs = ActivePropDefinitions;
            if (activeDefs == null || activeDefs.Count == 0)
            {
                Debug.LogWarning("[BackgroundPropSpawner] No prop definitions assigned");
                return;
            }

            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            // Prepare shared runtime material for batching
            if (_propMaterial != null)
            {
                if (_sharedRuntimeMaterial == null || _sharedRuntimeMaterial.shader != _propMaterial.shader)
                    _sharedRuntimeMaterial = new Material(_propMaterial);
            }
            else
            {
                _sharedRuntimeMaterial = null;
            }

            ClearOldProps();
            _usedPositions.Clear();

            var spline = _levelField.spline;
            int pointCount = spline.GetPointCount();
            if (pointCount < 2)
                return;

            bool isOpen = spline.isOpenEnded;

            // Build polygon in world space for "is inside level?" test
            // Only valid if it's a closed shape with >=3 points
            _cachedLevelPolygon.Clear();
            _cachedPolygonValid = !isOpen && pointCount >= 3;
            if (_cachedPolygonValid)
            {
                for (int idx = 0; idx < pointCount; idx++)
                {
                    Vector2 pL = spline.GetPosition(idx);
                    Vector2 pW = _levelField.transform.TransformPoint(pL);
                    _cachedLevelPolygon.Add(pW);
                }
            }

            // Find "center" for flipping props (lean toward center)
            Vector3 levelCenterWorld;
            {
                var rend = _levelField.GetComponent<SpriteShapeRenderer>();
                if (rend != null)
                    levelCenterWorld = rend.bounds.center;
                else
                    levelCenterWorld = _levelField.transform.position;
            }

            // -------- 1) Spawn props around LEVEL SPLINE border --------
            for (int i = 0; i < pointCount; i++)
            {
                int nextI = i + 1;
                if (nextI >= pointCount)
                {
                    if (isOpen) break;
                    nextI = 0;
                }

                Vector2 p0L = spline.GetPosition(i);
                Vector2 p1L = spline.GetPosition(nextI);

                Vector2 p0W = _levelField.transform.TransformPoint(p0L);
                Vector2 p1W = _levelField.transform.TransformPoint(p1L);

                Vector2 segVecW = p1W - p0W;
                float segLen = segVecW.magnitude;
                if (segLen < _minSegmentLengthForSpawn)
                    continue;

                Vector2 dirW = segVecW.normalized;
                // outward-ish normal (rotate 90° CCW)
                Vector2 outwardNormalW = new Vector2(-dirW.y, dirW.x).normalized;

                float dotDown = Vector2.Dot(outwardNormalW, Vector2.down);
                bool isFrontEdge = (dotDown >= _frontDotDownThreshold);

                float step = Mathf.Max(_spacing, 0.001f);
                float traveled = 0f;
                int spawnedOnThisSegment = 0;

                while (traveled <= segLen + 0.0001f && spawnedOnThisSegment < _maxPerSegment)
                {
                    if (Random.value > _spawnChance)
                    {
                        traveled += step;
                        continue;
                    }

                    float tNorm = (segLen <= 0.0001f) ? 0f : (traveled / segLen);
                    tNorm = Mathf.Clamp01(tNorm);

                    Vector2 basePosW = Vector2.Lerp(p0W, p1W, tNorm);

                    // push OUTWARD from level border
                    float distOut = Random.Range(_spawnDistanceRange.x, _spawnDistanceRange.y);
                    Vector2 spawnPosW = basePosW + outwardNormalW * distOut;

                    // pick sprite
                    PropDefinition def;
                    Sprite chosenSprite;
                    if (!PickRandomProp(out def, out chosenSprite))
                    {
                        traveled += step;
                        continue;
                    }

                    // jitter
                    Vector2 jitter = new Vector2(
                        Random.Range(-def.positionJitterRange.x, def.positionJitterRange.x),
                        Random.Range(-def.positionJitterRange.y, def.positionJitterRange.y)
                    );
                    Vector2 finalSpawnPosW = spawnPosW + jitter;

                    // global Y offset
                    Vector2 finalPosWithOffset = finalSpawnPosW + Vector2.up * _yOffset;

                    // anti-clump (root position)
                    if (IsTooCloseToExisting(finalPosWithOffset))
                    {
                        traveled += step;
                        continue;
                    }

                    // spawn actual GameObject and cull if overlapping the field
                    bool kept = SpawnPropGO(
                        def,
                        chosenSprite,
                        finalPosWithOffset,
                        levelCenterWorld,
                        dotDown,
                        isFrontEdge,
                        distOut
                    );

                    if (kept)
                    {
                        _usedPositions.Add(finalPosWithOffset);
                        spawnedOnThisSegment++;
                    }

                    traveled += step;
                }
            }

            // -------- 2) Spawn props near CAMERA SCREEN CORNERS --------
            if (_spawnScreenCorners && _camera != null)
            {
                Vector2 bl = ViewportToWorld2D(0f, 0f);
                Vector2 br = ViewportToWorld2D(1f, 0f);
                Vector2 tr = ViewportToWorld2D(1f, 1f);
                Vector2 tl = ViewportToWorld2D(0f, 1f);

                // bottom-left: inward (+x, +y)
                SpawnCornerCluster(bl, +1f, +1f, true, levelCenterWorld);
                // bottom-right: inward (-x, +y)
                SpawnCornerCluster(br, -1f, +1f, true, levelCenterWorld);
                // top-left: inward (+x, -y)
                SpawnCornerCluster(tl, +1f, -1f, false, levelCenterWorld);
                // top-right: inward (-x, -y)
                SpawnCornerCluster(tr, -1f, -1f, false, levelCenterWorld);
            }
        }

        // -------------------------------------------------
        // Corner helper
        // -------------------------------------------------
        private void SpawnCornerCluster(
            Vector2 cornerWorld,
            float inwardSignX,
            float inwardSignY,
            bool isBottomCorner,
            Vector3 centerGuess
        )
        {
            // bottom corners ~ "front" (bigger scale, front sorting)
            // top corners ~ "back" (smaller scale, back sorting)
            float fakeDotDown = isBottomCorner ? 1f : -1f;
            bool fakeIsFrontEdge = isBottomCorner;

            for (int k = 0; k < _cornerPropsPerCorner; k++)
            {
                float dx = Random.Range(0f, _cornerRadius) * inwardSignX;
                float dy = Random.Range(0f, _cornerRadius) * inwardSignY;

                Vector2 rawPos = cornerWorld + new Vector2(dx, dy);

                Vector2 finalPosWithOffset = rawPos + Vector2.up * _yOffset;

                if (IsTooCloseToExisting(finalPosWithOffset))
                    continue;

                PropDefinition def;
                Sprite chosenSprite;
                if (!PickRandomProp(out def, out chosenSprite))
                    continue;

                float fakeDist = _spawnDistanceRange.y;

                bool kept = SpawnPropGO(
                    def,
                    chosenSprite,
                    finalPosWithOffset,
                    centerGuess,
                    fakeDotDown,
                    fakeIsFrontEdge,
                    fakeDist
                );

                if (kept)
                {
                    _usedPositions.Add(finalPosWithOffset);
                }
            }
        }

        // -------------------------------------------------
        // Utility helpers
        // -------------------------------------------------

        private Vector2 ViewportToWorld2D(float vx, float vy)
        {
            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            Vector3 w = _camera.ViewportToWorldPoint(
                new Vector3(vx, vy, _camera.nearClipPlane)
            );
            return new Vector2(w.x, w.y);
        }

        // anti-clump check for root position
        private bool IsTooCloseToExisting(Vector2 candidatePos)
        {
            float minSq = _minSeparation * _minSeparation;
            for (int i = 0; i < _usedPositions.Count; i++)
            {
                if ((_usedPositions[i] - candidatePos).sqrMagnitude < minSq)
                    return true;
            }

            return false;
        }

        // Pick a random prop def + sprite
        private bool PickRandomProp(out PropDefinition def, out Sprite chosenSprite)
        {
            def = null;
            chosenSprite = null;

            var pool = ActivePropDefinitions;
            if (pool == null || pool.Count == 0)
                return false;

            for (int tries = 0; tries < 8; tries++)
            {
                PropDefinition d = pool[Random.Range(0, pool.Count)];
                if (d == null || d.sprites == null || d.sprites.Length == 0)
                    continue;

                Sprite s = d.sprites[Random.Range(0, d.sprites.Length)];
                if (s == null)
                    continue;

                def = d;
                chosenSprite = s;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Instantiates, scales, sorts, applies per-instance shader data,
        /// THEN checks overlap with the playable field.
        ///
        /// Returns true if the prop is kept.
        /// Returns false if culled (destroyed because it overlaps the field visually).
        /// </summary>
        private bool SpawnPropGO(
            PropDefinition def,
            Sprite chosenSprite,
            Vector2 worldPos,
            Vector3 levelCenterWorld,
            float dotDown,
            bool isFrontEdge,
            float distFromBorder
        )
        {
            // Create object
            GameObject propGO = new GameObject($"BGProp_{CHILD_TAG}");
            propGO.transform.SetParent(_spawnParent, worldPositionStays: false);
            propGO.transform.position = worldPos;

            // small Z-twist for variation
            float randomSmallTwist = Random.Range(-5f, 5f);
            propGO.transform.rotation = Quaternion.Euler(0f, 0f, randomSmallTwist);

            // parallax-ish scale
            float tBottom = Mathf.InverseLerp(-1f, 1f, dotDown);
            float parallaxScale = Mathf.Lerp(_topScale, _bottomScale, tBottom);

            float randMul = Random.Range(def.randomScaleRange.x, def.randomScaleRange.y);
            float finalScale = parallaxScale * randMul;

            Vector3 localScale = new Vector3(finalScale, finalScale, 1f);

            // Lean toward center? Flip X if needed
            if (def.faceLevelCenter)
            {
                bool centerIsLeft = (levelCenterWorld.x >= worldPos.x);
                localScale.x = centerIsLeft ? -finalScale : finalScale;
            }

            propGO.transform.localScale = localScale;

            // Renderer
            var sr = propGO.AddComponent<SpriteRenderer>();
            sr.sprite = chosenSprite;
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder = isFrontEdge ? _frontSortingOrder : _backSortingOrder;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;

            // shared material instance (for batching)
            if (_sharedRuntimeMaterial != null)
                sr.sharedMaterial = _sharedRuntimeMaterial;

            // per-instance overrides
            var mpb = new MaterialPropertyBlock();
            sr.GetPropertyBlock(mpb);

            // DistanceFactor
            if (!string.IsNullOrEmpty(_distanceShaderFloatName))
            {
                float distanceFactor01 = 0f;
                if (_spawnDistanceRange.y > 0.0001f)
                {
                    distanceFactor01 = Mathf.Clamp01(
                        (distFromBorder - _spawnDistanceRange.x) /
                        Mathf.Max(0.0001f, _spawnDistanceRange.y - _spawnDistanceRange.x) * _blurForce
                    );
                }

                mpb.SetFloat(_distanceShaderFloatName, distanceFactor01);
            }

            // WindFactor
            if (!string.IsNullOrEmpty(_windShaderFloatName))
            {
                float windVal = def.affectedByWind
                    ? Random.Range(_windRandomRange.x, _windRandomRange.y)
                    : 0f;
                mpb.SetFloat(_windShaderFloatName, windVal);
            }

            sr.SetPropertyBlock(mpb);

            // ---------------------------------------
            // CULL LOGIC: check if sprite overlaps field
            // ---------------------------------------
            // We now test THREE vertical samples on the sprite bounds in world space:
            // 1) top    = (center.x, max.y)
            // 2) middle = (center.x, center.y)
            // 3) bottom = (center.x, min.y)
            // If ANY of these lie inside the level polygon, we destroy the prop.
            Bounds wb = sr.bounds;

            Vector2 topCheckPoint    = new Vector2(wb.center.x, wb.max.y);
            Vector2 midCheckPoint    = new Vector2(wb.center.x, wb.center.y);
            Vector2 bottomCheckPoint = new Vector2(wb.center.x, wb.min.y);

            bool overlapsFieldTop    = _cachedPolygonValid && IsPointInsidePolygon(topCheckPoint, _cachedLevelPolygon);
            bool overlapsFieldMid    = _cachedPolygonValid && IsPointInsidePolygon(midCheckPoint, _cachedLevelPolygon);
            bool overlapsFieldBottom = _cachedPolygonValid && IsPointInsidePolygon(bottomCheckPoint, _cachedLevelPolygon);

            bool overlapsField = overlapsFieldTop || overlapsFieldMid || overlapsFieldBottom;

            if (overlapsField)
            {
                // kill it and report that we did not keep it
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(propGO);
                else
#endif
                    Object.Destroy(propGO);

                if (_debugLogs)
                {
                    Debug.Log($"[BackgroundPropSpawner] Culled prop {def.id} because it overlapped level.");
                }

                return false;
            }

            if (_debugLogs)
            {
                Debug.Log(
                    $"[BackgroundPropSpawner] Spawned {def.id} at {worldPos} | " +
                    $"dotDown={dotDown:0.00} front={isFrontEdge} scale={finalScale:0.00}"
                );
            }

            return true;
        }

        /// <summary>
        /// Raycast-style point-in-polygon (odd-even rule).
        /// Assumes polygon is non-self-intersecting, in world space, closed loop.
        /// </summary>
        private bool IsPointInsidePolygon(Vector2 p, List<Vector2> poly)
        {
            bool inside = false;
            int count = poly.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Vector2 pi = poly[i];
                Vector2 pj = poly[j];

                bool intersect = ((pi.y > p.y) != (pj.y > p.y)) &&
                                 (p.x < (pj.x - pi.x) * (p.y - pi.y) /
                                  (pj.y - pi.y + 0.000001f) + pi.x);
                if (intersect)
                    inside = !inside;
            }

            return inside;
        }

        /// <summary>
        /// Clears old props (children whose name contains CHILD_TAG) so we can regenerate cleanly.
        /// </summary>
        private void ClearOldProps()
        {
            if (_spawnParent == null)
                return;

            List<GameObject> toKill = new List<GameObject>();
            foreach (Transform c in _spawnParent)
            {
                if (c && c.name.Contains(CHILD_TAG))
                    toKill.Add(c.gameObject);
            }

            for (int i = 0; i < toKill.Count; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(toKill[i]);
                else
#endif
                    Object.Destroy(toKill[i]);
            }
        }

        // Debug gizmos in Scene view
        private void OnDrawGizmosSelected()
        {
            // Draw spline + outward normals
            if (_levelField != null)
            {
                var spline = _levelField.spline;
                int pointCount = spline.GetPointCount();
                if (pointCount >= 2)
                {
                    bool isOpen = spline.isOpenEnded;
                    Gizmos.color = Color.yellow;

                    for (int i = 0; i < pointCount; i++)
                    {
                        int nextI = i + 1;
                        if (nextI >= pointCount)
                        {
                            if (isOpen) break;
                            nextI = 0;
                        }

                        Vector2 p0L = spline.GetPosition(i);
                        Vector2 p1L = spline.GetPosition(nextI);

                        Vector2 p0W = _levelField.transform.TransformPoint(p0L);
                        Vector2 p1W = _levelField.transform.TransformPoint(p1L);

                        Gizmos.DrawLine(p0W, p1W);

                        Vector2 dirW = (p1W - p0W).normalized;
                        Vector2 outwardNormalW = new Vector2(-dirW.y, dirW.x).normalized;

                        Vector2 mid = (p0W + p1W) * 0.5f;
                        Gizmos.DrawLine(mid, mid + outwardNormalW * _spawnDistanceRange.y);
                    }
                }

                // also draw level polygon if closed
                if (!spline.isOpenEnded && pointCount >= 3)
                {
                    Gizmos.color = Color.green;
                    for (int i = 0; i < pointCount; i++)
                    {
                        int j = (i + 1) % pointCount;
                        Vector2 piL = spline.GetPosition(i);
                        Vector2 pjL = spline.GetPosition(j);
                        Vector2 piW = _levelField.transform.TransformPoint(piL);
                        Vector2 pjW = _levelField.transform.TransformPoint(pjL);
                        Gizmos.DrawLine(piW, pjW);
                    }
                }
            }

            // Draw camera corner spawn radii
            if (_spawnScreenCorners)
            {
                if (_camera == null)
                    _camera = UnityEngine.Camera.main;

                if (_camera != null)
                {
                    Gizmos.color = Color.cyan;

                    Vector2 bl = ViewportToWorld2D(0f, 0f);
                    Vector2 br = ViewportToWorld2D(1f, 0f);
                    Vector2 tr = ViewportToWorld2D(1f, 1f);
                    Vector2 tl = ViewportToWorld2D(0f, 1f);

                    Gizmos.DrawWireSphere(bl, _cornerRadius);
                    Gizmos.DrawWireSphere(br, _cornerRadius);
                    Gizmos.DrawWireSphere(tr, _cornerRadius);
                    Gizmos.DrawWireSphere(tl, _cornerRadius);
                }
            }
        }
    }
}

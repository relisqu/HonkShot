using Sirenix.OdinInspector;

namespace Scripts.LevelSystem.TechnicalScripts
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.U2D;

    [ExecuteAlways]
    public class CornerGenerator : MonoBehaviour
    {
        [System.Serializable]
        public class CornerVariant
        {
            public Sprite sprite;

            [Tooltip("Desired dot(avgNormal, Vector2.down) this variant is 'meant' for. 1 = straight down, 0 = side, -1 = up.")]
            [Range(-1f, 1f)] public float targetDotDown = 1f;
        }

        [Header("Source Shape")] 
        [SerializeField] private SpriteShapeController spriteShape;

        [Header("Corner Decoration")]
        [Tooltip("Sprite variants. We'll select the one with targetDotDown closest to the actual corner's dotDown.")]
        [SerializeField] private List<CornerVariant> cornerVariants = new List<CornerVariant>();

        [Tooltip("All spawned corner sprites will be parented under this transform.")]
        [SerializeField] private Transform spawnParent;

        [Tooltip("How strongly the corner's averaged normal must point downward to count as 'bottom'. 1=only perfectly down, 0=anything.")]
        [Range(-1f, 1f)]
        [SerializeField] private float bottomDotThreshold = 0.5f;

        [Tooltip("How far to push spawned sprite outward along the averaged normal (world units).")]
        [SerializeField] private float pushOut = 0.05f;

        [Tooltip("Extra LOCAL offset after pushOut (applied in the SpriteShape's local space).")]
        [SerializeField] private Vector2 extraOffset = Vector2.zero;

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 0;

        [Header("Runtime")]
        [Tooltip("Automatically regenerate in Awake(). Turn off if you only want manual runs in editor.")]
        [SerializeField] private bool autoRegenOnAwake = true;

        [Header("Angle-based behaviour")]
        [Tooltip("Flip X if world normal points left.")]
        [SerializeField] private bool mirrorOnLeft = true;

        [Tooltip("Sideways deflection amount (world units at maximum). Multiplied by normal.x (i.e., more left/right = more deflect).")]
        [SerializeField, Min(0f)] private float sideDeflectByAngle = 0.06f;

        [Tooltip("Base local scale for spawned sprites (before X-mirror).")]
        [SerializeField] private Vector3 baseScale = Vector3.one;

        [Tooltip("Enable debug logs for per-corner dotDown & picks.")]
        [SerializeField] private bool debugLogs = false;

        private const string CHILD_TAG = "_cornerDecor";

        private void Awake()
        {
            if (autoRegenOnAwake) Generate();
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteShape == null)
                spriteShape = GetComponent<SpriteShapeController>();
        }
    #endif

        [ContextMenu("Generate Corner Decos")]
        [Button]
        public void Generate()
        {
            if (spriteShape == null) { Debug.LogWarning("[CornerGenerator] No SpriteShapeController assigned."); return; }
            if (cornerVariants == null || cornerVariants.Count == 0) { Debug.LogWarning("[CornerGenerator] No cornerVariants assigned."); return; }
            if (spawnParent == null) { Debug.LogWarning("[CornerGenerator] No spawnParent assigned."); return; }

            ClearOldChildren();

            var spline = spriteShape.spline;
            int pointCount = spline.GetPointCount();
            if (pointCount < 3) return;

            // Cache transform refs
            var shapeTr = spriteShape.transform;
            var parentTr = spawnParent;

            for (int i = 0; i < pointCount; i++)
            {
                // LOCAL positions from spline
                Vector2 prevL = spline.GetPosition((i - 1 + pointCount) % pointCount);
                Vector2 currL = spline.GetPosition(i);
                Vector2 nextL = spline.GetPosition((i + 1) % pointCount);

                Vector2 dirPrevL = (currL - prevL).normalized;
                Vector2 dirNextL = (nextL - currL).normalized;
                if (dirPrevL.sqrMagnitude < 1e-6f || dirNextL.sqrMagnitude < 1e-6f)
                    continue;

                // Per-edge normals (LOCAL). (-y, x) = +90° CCW.
                Vector2 n1L = new Vector2(-dirPrevL.y, dirPrevL.x);
                Vector2 n2L = new Vector2(-dirNextL.y, dirNextL.x);

                // Averaged corner normal (LOCAL)
                Vector2 avgNormalL = (n1L + n2L);
                float mag = avgNormalL.magnitude;
                if (mag < 1e-6f) continue; // nearly straight line
                avgNormalL /= mag;

                // Convert to WORLD for robust dot/angle decisions
                Vector2 avgNormalW = (Vector2)shapeTr.TransformDirection(new Vector3(avgNormalL.x, avgNormalL.y, 0f)).normalized;

                // Down-facing check in WORLD
                float dotDown = Vector2.Dot(avgNormalW, Vector2.down);
                if (debugLogs) Debug.Log($"[CornerGenerator] dotDown={dotDown:0.000} at i={i}");
                if (dotDown < bottomDotThreshold) continue;

                // --- Pick best variant by closest targetDotDown ---
                var (variant, variantIndex) = PickVariantByDot(dotDown);
                if (variant == null || variant.sprite == null) continue;
                if (debugLogs) Debug.Log($"[CornerGenerator] pick idx={variantIndex} sprite={variant.sprite.name}");

                // Tangent (LOCAL) = +90° CW from normal to slide sideways
                Vector2 tangentL = new Vector2(avgNormalL.y, -avgNormalL.x);

                // Compute LOCAL final position
                Vector2 finalLocal = currL 
                                     + avgNormalL * pushOut                 // outward
                                     + tangentL  * (sideDeflectByAngle * avgNormalW.x) // sideways deflect scales with world normal.x
                                     + extraOffset;

                // Convert to WORLD and spawn
                Vector3 finalWorld = shapeTr.TransformPoint(finalLocal);

                var cornerObj = new GameObject($"CornerDeco_{i}{CHILD_TAG}");
                cornerObj.transform.SetParent(parentTr, worldPositionStays: false);
                cornerObj.transform.position = finalWorld;

                // WORLD rotation: face sprite's +Y along world normal
                float angleDeg = Mathf.Atan2(avgNormalW.y, avgNormalW.x) * Mathf.Rad2Deg - 90f;
               // cornerObj.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

                // Mirror on left if desired
                Vector3 scale = baseScale;
                if (mirrorOnLeft && avgNormalW.x < 0f) scale.x *= -1f;
                cornerObj.transform.localScale = scale;

                // Renderer
                var sr = cornerObj.AddComponent<SpriteRenderer>();
                sr.sprite = variant.sprite;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;
            }
        }

        private (CornerVariant variant, int index) PickVariantByDot(float actualDotDown)
        {
            CornerVariant best = null;
            int bestIdx = -1;
            float bestScore = float.PositiveInfinity;

            for (int i = 0; i < cornerVariants.Count; i++)
            {
                var v = cornerVariants[i];
                if (v == null || v.sprite == null) continue;

                float score = Mathf.Abs(v.targetDotDown - actualDotDown);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = v;
                    bestIdx = i;
                }
            }
            return (best, bestIdx);
        }

        private void ClearOldChildren()
        {
            var toDestroy = new List<GameObject>();

            foreach (Transform child in spawnParent)
            {
                if (child && child.name.Contains(CHILD_TAG))
                    toDestroy.Add(child.gameObject);
            }

            for (int i = 0; i < toDestroy.Count; i++)
            {
    #if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(toDestroy[i]);
                else
    #endif
                    Destroy(toDestroy[i]);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Scripts.LevelSystem.LevelObjects
{
    /// <summary>
    /// Marks this GameObject as a small island. Registers a world-space polygon so that
    /// systems like BackgroundPropSpawner can cull decorations that would otherwise sit
    /// on top of the island.
    ///
    /// The polygon is derived from (in priority order):
    ///   1. An explicit SpriteShapeController override.
    ///   2. An explicit PolygonCollider2D override.
    ///   3. The first matching component found on this GameObject.
    ///   4. Fallback: the renderer's bounds as a 4-corner rectangle.
    /// </summary>
    public class SmallIsland : MonoBehaviour
    {
        public static readonly HashSet<SmallIsland> ActiveInstances = new HashSet<SmallIsland>();

        [Tooltip("Optional: override the boundary source. Leave empty to auto-detect on this GameObject.")]
        [SerializeField] private SpriteShapeController _spriteShape;
        [SerializeField] private PolygonCollider2D _polygonCollider;
        [SerializeField] private Renderer _boundsFallback;

        private void OnEnable()
        {
            ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }

        private void EnsureBoundsSource()
        {
            if (!_spriteShape) _spriteShape = GetComponentInChildren<SpriteShapeController>(true);
            if (!_polygonCollider) _polygonCollider = GetComponentInChildren<PolygonCollider2D>(true);
            if (!_boundsFallback) _boundsFallback = GetComponentInChildren<Renderer>(true);
        }

        public List<Vector2> BuildWorldPolygon()
        {
            EnsureBoundsSource();

            if (_spriteShape)
            {
                var spline = _spriteShape.spline;
                int count = spline.GetPointCount();
                if (!spline.isOpenEnded && count >= 3)
                {
                    var list = new List<Vector2>(count);
                    var t = _spriteShape.transform;
                    for (int i = 0; i < count; i++)
                        list.Add(t.TransformPoint(spline.GetPosition(i)));
                    return list;
                }
            }

            if (_polygonCollider)
            {
                var points = _polygonCollider.points;
                if (points != null && points.Length >= 3)
                {
                    var list = new List<Vector2>(points.Length);
                    var t = _polygonCollider.transform;
                    for (int i = 0; i < points.Length; i++)
                        list.Add(t.TransformPoint(points[i] + _polygonCollider.offset));
                    return list;
                }
            }

            if (_boundsFallback)
            {
                var b = _boundsFallback.bounds;
                return new List<Vector2>(4)
                {
                    new Vector2(b.min.x, b.min.y),
                    new Vector2(b.max.x, b.min.y),
                    new Vector2(b.max.x, b.max.y),
                    new Vector2(b.min.x, b.max.y),
                };
            }

            return null;
        }
    }
}

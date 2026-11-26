#if UNITY_EDITOR
using System.Collections.Generic;
using Scripts.Enemies;
using UnityEditor;
using UnityEngine;

namespace Scripts.LevelLab
{
    // Gives every walker a patrol route that fits the room: an explicit recipe path, or the best-fitting
    // generated line/triangle around its recipe position.
    public static class LevelLabWalkerPaths
    {
        public static float Clearance = 0.9f;
        public static float SpawnClearance = 4.2f;
        public static float LineHalfLength = 2f;
        public static float SampleStep = 0.25f;
        public static readonly float[] Scales = { 1f, 0.75f, 0.55f };
        public static readonly Vector2[] TriangleShape = { new(0f, 0f), new(1.6f, 2.05f), new(4.14f, 0.22f) };

        public struct Obstacle
        {
            public Vector2 A;
            public Vector2 B;
            public float Radius;
        }

        public class Context
        {
            public List<Vector2> Outline;
            public Vector2 Spawn;
            public List<Obstacle> Obstacles = new();
        }

        public static bool IsWalkerKey(string key) => key == "Walker" || key == "WalkerSpot";

        public static Context BuildContext(RoomRecipe recipe, List<Vector2> outline)
        {
            var ctx = new Context { Outline = outline, Spawn = RoomRecipe.ToVector(recipe.spawn, Vector2.zero) };
            if (recipe.objects == null) return ctx;
            foreach (var o in recipe.objects)
            {
                if (TryFootprint(o, out var obstacle)) ctx.Obstacles.Add(obstacle);
            }

            return ctx;
        }

        private static bool TryFootprint(RecipeObject o, out Obstacle obstacle)
        {
            obstacle = new Obstacle { A = o.Position, B = o.Position };
            float barAngle;
            switch (o.key)
            {
                case "BouncerV": barAngle = o.rot; break;
                case "Bouncer1": barAngle = o.rot + 90f; break;
                case "Bouncer": barAngle = o.rot + 27f; break;
                case "Bouncer2": obstacle.Radius = 0.7f; return true;
                case "Bouncer3": obstacle.Radius = 0.55f; return true;
                case "Bouncer4": obstacle.Radius = 0.65f; return true;
                case "BouncerV1": obstacle.Radius = 0.8f; return true;
                case "SphereBouncer": obstacle.Radius = 0.75f; return true;
                case "Cannon": obstacle.Radius = 0.5f; return true;
                case "Hole": obstacle.Radius = 0.6f; return true;
                case "InnerWall":
                    obstacle.A = obstacle.B = o.Position + new Vector2(-0.66f, 0.2f);
                    obstacle.Radius = 2.6f;
                    return true;
                default: return false;
            }

            var half = (Vector2)(Quaternion.Euler(0, 0, barAngle) * Vector3.right) * 0.97f * Mathf.Max(1f, o.Scale.x);
            obstacle.A = o.Position - half;
            obstacle.B = o.Position + half;
            obstacle.Radius = 0.25f;
            return true;
        }

        public static List<Vector2> ChoosePath(RecipeObject o, Context ctx, out bool closed)
        {
            var explicitPath = o.PathPoints();
            if (explicitPath.Count >= 2)
            {
                closed = explicitPath.Count >= 3;
                return explicitPath;
            }

            bool triangle = o.key == "WalkerSpot";
            closed = triangle;
            List<Vector2> best = null;
            float bestScore = float.MinValue;
            foreach (float scale in Scales)
            {
                for (int step = 0; step < 12; step++)
                {
                    var candidate = triangle ? Triangle(o.Position, step * 30f, scale) : Line(o.Position, step * 15f, scale);
                    float score = Score(candidate, closed, ctx) + scale * 0.3f;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }

                if (bestScore >= Clearance) break;
            }

            return StartFarthestFromSpawn(best, closed, ctx.Spawn);
        }

        private static List<Vector2> StartFarthestFromSpawn(List<Vector2> path, bool closed, Vector2 spawn)
        {
            if (path == null) return null;
            int far = 0;
            for (int i = 1; i < path.Count; i++)
                if (Vector2.Distance(path[i], spawn) > Vector2.Distance(path[far], spawn)) far = i;
            if (far == 0) return path;
            if (!closed)
            {
                path.Reverse();
                return path;
            }

            var rotated = new List<Vector2>();
            for (int i = 0; i < path.Count; i++) rotated.Add(path[(far + i) % path.Count]);
            return rotated;
        }

        private static List<Vector2> Line(Vector2 centre, float angle, float scale)
        {
            var half = (Vector2)(Quaternion.Euler(0, 0, angle) * Vector3.right) * LineHalfLength * scale;
            return new List<Vector2> { centre - half, centre + half };
        }

        private static List<Vector2> Triangle(Vector2 centre, float angle, float scale)
        {
            var centroid = (TriangleShape[0] + TriangleShape[1] + TriangleShape[2]) / 3f;
            var rotation = Quaternion.Euler(0, 0, angle);
            var points = new List<Vector2>();
            foreach (var p in TriangleShape)
                points.Add(centre + (Vector2)(rotation * (Vector3)((p - centroid) * scale)));
            return points;
        }

        // Smallest clearance along the route; negative when it leaves the room or crosses the spawn zone.
        public static float Score(List<Vector2> path, bool closed, Context ctx)
        {
            float min = float.MaxValue;
            int segments = closed ? path.Count : path.Count - 1;
            for (int i = 0; i < segments; i++)
            {
                var a = path[i];
                var b = path[(i + 1) % path.Count];
                int samples = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) / SampleStep));
                for (int s = 0; s <= samples; s++)
                {
                    var p = Vector2.Lerp(a, b, s / (float)samples);
                    if (!LevelLabValidator.InsidePolygon(p, ctx.Outline)) return -1f;
                    if (Vector2.Distance(p, ctx.Spawn) < SpawnClearance) return -0.5f;
                    min = Mathf.Min(min, DistanceToOutline(p, ctx.Outline));
                    foreach (var o in ctx.Obstacles)
                        min = Mathf.Min(min, DistancePointSegment(p, o.A, o.B) - o.Radius);
                }
            }

            return min;
        }

        public static void Apply(GameObject walkerSpot, List<Vector2> path, bool closed, string recipeId)
        {
            var walker = walkerSpot.GetComponentInChildren<WalkingEnemy>(true);
            var spots = walkerSpot.transform.Find("Spots");
            if (!walker || !spots || path == null || path.Count < 2)
            {
                Debug.LogError($"[LevelLab] {recipeId}: can't set a walker path on '{walkerSpot.name}'");
                return;
            }

            float z = walkerSpot.transform.position.z;
            walkerSpot.transform.position = new Vector3(path[0].x, path[0].y, z);
            spots.localPosition = Vector3.zero;
            walker.transform.localPosition = Vector3.zero;

            while (spots.childCount < path.Count)
            {
                var extra = new GameObject($"Spot ({spots.childCount})");
                extra.transform.SetParent(spots, false);
            }

            var so = new SerializedObject(walker);
            var points = so.FindProperty("_pathPoints");
            points.arraySize = path.Count;
            for (int i = 0; i < spots.childCount; i++)
            {
                var spot = spots.GetChild(i);
                var p = i < path.Count ? path[i] : path[0];
                spot.position = new Vector3(p.x, p.y, z);
                if (i < path.Count) points.GetArrayElementAtIndex(i).objectReferenceValue = spot;
            }

            so.FindProperty("_walkingType").enumValueIndex = (int)(closed ? WalkingType.Closed : WalkingType.YoYo);
            so.FindProperty("_currentTargetIndex").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static float DistanceToOutline(Vector2 p, List<Vector2> poly)
        {
            float best = float.MaxValue;
            for (int i = 0; i < poly.Count; i++)
                best = Mathf.Min(best, DistancePointSegment(p, poly[i], poly[(i + 1) % poly.Count]));
            return best;
        }

        private static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
#endif

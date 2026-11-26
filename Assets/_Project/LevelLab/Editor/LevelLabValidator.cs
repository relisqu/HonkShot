#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scripts.Enemies;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.LevelObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Scripts.LevelLab
{
    public static class LevelLabValidator
    {
        public const string ReportsFolder = "Assets/_Project/LevelLab/Reports";
        private const string Floor1ConfigPath = "Assets/_Project/Resources/Data/Floors/FloorConfig_01.asset";
        private static readonly Vector3 TestOffset = new Vector3(0, 1000, 0);

        public static float MinEnemySpawnDistance = 4f;
        public static float MinOutlineAngle = 60f;
        public static float MinCorridorWidth = 2.5f;
        // Gaps narrower than the ball's diameter can't be entered, so they act as a sealed wall.
        public static float AttachedGap = 0.75f;
        public static float MinFreeGap = 2.1f;
        public static float MinBounciness = 0.85f;
        public static float EntrySightRange = 6f;
        public static float AreaTolerance = 0.2f;
        public static float TiltTolerance = 3f;
        public static float SharpCornerAngle = 110f;
        public static float SharpCornerClearance = 3f;
        public static float SweepSpeed = 20f;
        public static float SweepSpacing = 1.5f;
        public static float SweepInset = 0.6f;
        public static float[] SweepAngles = { 6f, 18f, 35f };
        public static int SweepRandomShots = 80;
        public static float TrapWindow = 0.8f;
        public static int TrapContacts = 6;
        public static float TrapRadius = 1.2f;
        public static float TrapMergeDistance = 1.5f;
        public static int TrapErrorShots = 2;
        public static int PurposeContacts = 3;
        public static float PurposePointDistance = 0.9f;
        public static float CannonSweep = 30f;
        public static int CannonPurposeNeeded = 2;
        public static float WalkerRadius = 0.45f;
        public static float WalkerSpawnClearance = 3f;

        public class Report
        {
            public string Room;
            public float Area;
            public int Enemies;
            public int Danger;
            public int Sliders;
            public int Cannons;
            public int Points;
            public int SweepShots;
            public bool Chill => Danger == 0;
            public List<string> Errors = new List<string>();
            public List<string> Warnings = new List<string>();
            public bool Passed => Errors.Count == 0;
        }

        [MenuItem("Tools/LevelLab/Validate LevelLab Rooms")]
        public static void ValidateLevelLabMenu()
        {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelLabRoomBuilder.OutputFolder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => !Path.GetFileName(p).StartsWith("LL_00")).OrderBy(p => p).ToList();
            WriteReport(ValidatePaths(paths), "validation_levellab.txt");
        }

        [MenuItem("Tools/LevelLab/Validate Floor 1 Rooms")]
        public static void ValidateFloor1Menu()
        {
            WriteReport(ValidatePaths(Floor1RoomPaths()), "validation_floor1.txt");
        }

        public static List<string> Floor1RoomPaths()
        {
            var config = AssetDatabase.LoadAssetAtPath<FloorConfigSO>(Floor1ConfigPath);
            return config.RoomModels.Where(m => m != null && m.RoomPrefab)
                .Select(m => AssetDatabase.GetAssetPath(m.RoomPrefab)).Distinct().ToList();
        }

        public static List<Report> ValidatePaths(List<string> paths)
        {
            var floor1Areas = Floor1RoomPaths().Select(p => OutlineArea(AssetDatabase.LoadAssetAtPath<GameObject>(p))).ToList();
            float minArea = floor1Areas.Min() * (1 - AreaTolerance);
            float maxArea = floor1Areas.Max() * (1 + AreaTolerance);
            return paths.Select(p => Validate(AssetDatabase.LoadAssetAtPath<GameObject>(p), minArea, maxArea)).ToList();
        }

        private static void WriteReport(List<Report> reports, string fileName)
        {
            var sb = new StringBuilder();
            foreach (var r in reports)
            {
                sb.AppendLine($"{(r.Passed ? "PASS" : "FAIL")} {r.Room} area={r.Area:0} enemies={r.Enemies} danger={r.Danger}{(r.Chill ? " CHILL" : "")} sliders={r.Sliders} cannons={r.Cannons} points={r.Points} sweep={r.SweepShots}");
                foreach (var e in r.Errors) sb.AppendLine("   ERROR " + e);
                foreach (var w in r.Warnings) sb.AppendLine("   warn  " + w);
            }

            Directory.CreateDirectory(ReportsFolder);
            File.WriteAllText(Path.Combine(ReportsFolder, fileName), sb.ToString());
            Debug.Log($"[LevelLab] Validation: {reports.Count(r => r.Passed)}/{reports.Count} passed -> {ReportsFolder}/{fileName}\n{sb}");
        }

        public static List<Vector2> WorldOutline(GameObject room)
        {
            var field = room.transform.Find("Field");
            var spline = field.GetComponent<SpriteShapeController>().spline;
            var points = new List<Vector2>();
            for (int i = 0; i < spline.GetPointCount(); i++)
                points.Add(field.TransformPoint(spline.GetPosition(i)));
            return points;
        }

        private static float OutlineArea(GameObject roomPrefab)
        {
            return Mathf.Abs(LevelLabRoomBuilder.SignedArea(WorldOutline(roomPrefab)));
        }

        public static Report Validate(GameObject roomPrefab, float minArea, float maxArea)
        {
            var report = new Report { Room = roomPrefab.name };
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(roomPrefab);
            try
            {
                instance.transform.position = TestOffset;
                Physics2D.SyncTransforms();
                ValidateInstance(instance, report, minArea, maxArea);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            return report;
        }

        private static void ValidateInstance(GameObject room, Report report, float minArea, float maxArea)
        {
            var outline = WorldOutline(room).Select(p => p - (Vector2)TestOffset).ToList();
            report.Area = Mathf.Abs(LevelLabRoomBuilder.SignedArea(outline));
            if (report.Area < minArea || report.Area > maxArea)
                report.Warnings.Add($"area {report.Area:0} outside floor-1 range [{minArea:0}, {maxArea:0}]");

            CheckOutlineShape(outline, report);

            var spawn = (Vector2)(room.GetComponent<Room>().SpawnPointTransform.position - TestOffset);
            if (!InsidePolygon(spawn, outline)) report.Errors.Add("spawn point outside outline");
            else if (DistanceToOutline(spawn, outline) < 1f) report.Warnings.Add("spawn point closer than 1u to a wall");

            var levelObjects = room.transform.Find("LevelObjects");
            var walls = levelObjects.Find("Walls").GetComponentsInChildren<Collider2D>().Where(c => !c.isTrigger).ToList();
            var placed = new List<Transform>();
            foreach (Transform child in levelObjects)
                if (child.name != "Walls") placed.Add(child);

            var enemies = placed.Where(t => t.GetComponentInChildren<EnemyHealth>(true)).ToList();
            report.Enemies = enemies.Count;
            if (enemies.Count == 0) report.Errors.Add("no enemies");

            foreach (var t in placed)
            {
                var local = (Vector2)(t.position - TestOffset);
                if (!InsidePolygon(local, outline)) report.Errors.Add($"{t.name} at {local:0.0} is outside the outline");
            }

            foreach (var e in enemies)
            {
                var pos = (Vector2)(e.position - TestOffset);
                float d = Vector2.Distance(pos, spawn);
                if (d < MinEnemySpawnDistance) report.Errors.Add($"{e.name} only {d:0.0}u from spawn");
                if (IsRanged(e) && d < EntrySightRange && !Blocked(spawn, pos, walls, placed, e))
                    report.Warnings.Add($"{e.name} has open sight on the spawn from {d:0.0}u");
            }

            var solids = placed.Where(t => !t.GetComponentInChildren<EnemyHealth>(true))
                .Select(t => (t, cols: SolidColliders(t))).Where(x => x.cols.Count > 0).ToList();

            foreach (var (t, cols) in solids)
            {
                foreach (var bounce in t.GetComponentsInChildren<BounceObject>(true))
                    if (bounce.Bounciness < MinBounciness)
                        report.Errors.Add($"{t.name} has bounciness {bounce.Bounciness:0.00} (speed killer)");
            }

            for (int i = 0; i < solids.Count; i++)
            {
                float wallGap = MinDistance(solids[i].cols, walls);
                if (wallGap > AttachedGap && wallGap < MinFreeGap)
                    report.Errors.Add($"{solids[i].t.name} at {Local(solids[i].t):0.0} leaves a {wallGap:0.00}u gap to a wall (ball trap risk)");
                else if (wallGap < MinFreeGap && IsTilted(solids[i].t))
                    report.Errors.Add($"{solids[i].t.name} at {Local(solids[i].t):0.0} is tilted and {wallGap:0.00}u from a wall (V-wedge)");

                for (int j = i + 1; j < solids.Count; j++)
                {
                    float gap = MinDistance(solids[i].cols, solids[j].cols);
                    if (gap < -0.05f) report.Warnings.Add($"{solids[i].t.name} overlaps {solids[j].t.name}");
                    else if (gap > AttachedGap && gap < MinFreeGap)
                        report.Warnings.Add($"{solids[i].t.name} and {solids[j].t.name} leave a {gap:0.00}u gap");
                }
            }

            foreach (var e in enemies)
            {
                var cols = SolidColliders(e);
                if (cols.Count == 0) continue;
                if (MinDistance(cols, walls) < 0f) report.Errors.Add($"{e.name} overlaps a wall");
                foreach (var (t, solidCols) in solids)
                    if (MinDistance(cols, solidCols) < 0f) report.Errors.Add($"{e.name} overlaps {t.name}");
            }

            report.Danger = enemies.Count(e => !e.name.Contains("Fly")) + placed.Count(t => t.name.StartsWith("Hole"));
            if (!report.Chill && report.Danger < 2)
                report.Warnings.Add($"only {report.Danger} danger enemy (non-chill rooms want 2+)");

            CheckSharpCorners(outline, solids, report);
            CheckPurpose(placed, report);
            CheckWalkers(placed, outline, spawn, report);
            SweepForTraps(outline, report);
        }

        private static Vector2 Local(Transform t) => (Vector2)(t.position - TestOffset);

        private static bool IsTilted(Transform t)
        {
            foreach (var box in t.GetComponentsInChildren<BoxCollider2D>(true))
            {
                if (box.isTrigger) continue;
                float z = Mathf.Repeat(box.transform.eulerAngles.z, 90f);
                if (z > TiltTolerance && z < 90f - TiltTolerance) return true;
            }

            return false;
        }

        private static void CheckSharpCorners(List<Vector2> outline, List<(Transform t, List<Collider2D> cols)> solids, Report report)
        {
            float orientation = Mathf.Sign(LevelLabRoomBuilder.SignedArea(outline));
            int n = outline.Count;
            for (int i = 0; i < n; i++)
            {
                var a = outline[(i - 1 + n) % n] - outline[i];
                var b = outline[(i + 1) % n] - outline[i];
                float cross = a.x * b.y - a.y * b.x;
                bool convex = Mathf.Sign(cross) == -orientation;
                float angle = Vector2.Angle(a, b);
                if (!convex || angle >= SharpCornerAngle) continue;

                var corner = outline[i] + (Vector2)TestOffset;
                foreach (var (t, cols) in solids)
                {
                    float d = cols.Min(c => Vector2.Distance(c.ClosestPoint(corner), corner));
                    if (d < SharpCornerClearance)
                        report.Warnings.Add($"{t.name} at {Local(t):0.0} is {d:0.0}u from the {angle:0} deg corner at {outline[i]:0.0}");
                }
            }
        }

        private static LevelLabShotPlanner CreatePlanner(bool ignoreEnemies)
        {
            var player = Object.FindObjectOfType<Scripts.Player.PlayerBallMovement>();
            Rigidbody2D body = null;
            if (player)
            {
                body = player.GetComponentInParent<Rigidbody2D>();
                if (!body) body = player.GetComponentInChildren<Rigidbody2D>();
            }

            var ballCollider = body ? body.GetComponentInChildren<CircleCollider2D>() : null;
            int layer = ballCollider ? ballCollider.gameObject.layer : body ? body.gameObject.layer : LayerMask.NameToLayer("Player");
            return new LevelLabShotPlanner(null, Mathf.Max(0, layer))
            {
                Damping = body ? body.linearDamping : 0f,
                IgnoreEnemies = ignoreEnemies
            };
        }

        private static void CheckPurpose(List<Transform> placed, Report report)
        {
            var planner = CreatePlanner(false);
            planner.MaxContacts = PurposeContacts;
            planner.Targets.AddRange(placed.Where(t => t.name.StartsWith("PointObject") || t.name.StartsWith("Arrow")));
            report.Points = planner.Targets.Count;
            if (report.Points == 0) report.Warnings.Add("no PointObject/Arrow power pickups");

            foreach (var t in placed)
            {
                if (t.name.StartsWith("MoveRoad"))
                {
                    report.Sliders++;
                    if (!LevelLabRoomBuilder.TryGetRoadEnd(t.gameObject, out var end, out var dir))
                    {
                        report.Errors.Add($"{t.name}: slider has no usable path");
                        continue;
                    }

                    if (!Purposeful(planner, end + dir * 0.3f, dir))
                        report.Errors.Add($"{t.name}: slider exit at {end - (Vector2)TestOffset:0.0} leads to no enemy or power pickup");
                }
                else if (t.name.StartsWith("Cannon"))
                {
                    report.Cannons++;
                    int good = 0;
                    foreach (float sweep in new[] { -CannonSweep, 0f, CannonSweep })
                    {
                        var dir = (Vector2)(Quaternion.Euler(0, 0, t.eulerAngles.z + sweep) * Vector3.up);
                        if (Purposeful(planner, (Vector2)t.position + dir * 0.6f, dir)) good++;
                    }

                    if (good < CannonPurposeNeeded)
                        report.Errors.Add($"{t.name} at {Local(t):0.0}: only {good}/3 aim directions lead to an enemy or power pickup");
                }
            }
        }

        private static void CheckWalkers(List<Transform> placed, List<Vector2> outline, Vector2 spawn, Report report)
        {
            var offset = (Vector2)TestOffset;
            foreach (var t in placed)
            foreach (var walker in t.GetComponentsInChildren<WalkingEnemy>(true))
            {
                var so = new SerializedObject(walker);
                var list = so.FindProperty("_pathPoints");
                var route = new List<Vector2>();
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue is Transform p) route.Add((Vector2)p.position - offset);
                if (route.Count < 2)
                {
                    report.Errors.Add($"{t.name} at {Local(t):0.0} has no patrol route (stands still)");
                    continue;
                }

                bool closed = so.FindProperty("_walkingType").enumValueIndex == (int)WalkingType.Closed;
                int segments = closed ? route.Count : route.Count - 1;
                for (int i = 0; i < segments; i++)
                {
                    var a = route[i];
                    var b = route[(i + 1) % route.Count];
                    if (!InsidePolygon(a, outline)) report.Errors.Add($"{t.name} route point {a:0.0} is outside the room");
                    if (DistancePointSegment(spawn, a, b) < WalkerSpawnClearance)
                        report.Warnings.Add($"{t.name} route passes {DistancePointSegment(spawn, a, b):0.0}u from spawn");
                    var dir = b - a;
                    var hits = Physics2D.CircleCastAll(a + offset, WalkerRadius, dir.normalized, dir.magnitude);
                    foreach (var h in hits)
                    {
                        if (h.collider.isTrigger || h.collider.GetComponentInParent<EnemyHealth>()) continue;
                        if (h.collider.transform.IsChildOf(t)) continue;
                        report.Errors.Add($"{t.name} route {a:0.0}->{b:0.0} runs into {h.collider.transform.parent?.name ?? h.collider.name}");
                        break;
                    }
                }
            }
        }

        private static bool Purposeful(LevelLabShotPlanner planner, Vector2 origin, Vector2 dir)
        {
            var r = planner.Simulate(origin, dir * SweepSpeed);
            return r.EnemiesHit > 0 || r.MinTargetDistance < PurposePointDistance;
        }

        private static void SweepForTraps(List<Vector2> outline, Report report)
        {
            var planner = CreatePlanner(true);
            var offset = (Vector2)TestOffset;
            float orientation = Mathf.Sign(LevelLabRoomBuilder.SignedArea(outline));
            var shots = new List<(Vector2 origin, Vector2 dir)>();
            int n = outline.Count;
            for (int i = 0; i < n; i++)
            {
                var a = outline[i];
                var b = outline[(i + 1) % n];
                var tangent = (b - a).normalized;
                var inward = orientation > 0 ? new Vector2(-tangent.y, tangent.x) : new Vector2(tangent.y, -tangent.x);
                float length = Vector2.Distance(a, b);
                for (float s = SweepSpacing * 0.5f; s < length; s += SweepSpacing)
                {
                    var origin = a + tangent * s + inward * SweepInset;
                    foreach (float angle in SweepAngles)
                    foreach (var along in new[] { tangent, -tangent })
                    {
                        float rad = angle * Mathf.Deg2Rad;
                        shots.Add((origin, (along * Mathf.Cos(rad) + inward * Mathf.Sin(rad)).normalized));
                    }
                }
            }

            var rng = new System.Random(report.Room.GetHashCode());
            var min = new Vector2(outline.Min(p => p.x), outline.Min(p => p.y));
            var max = new Vector2(outline.Max(p => p.x), outline.Max(p => p.y));
            for (int k = 0, tries = 0; k < SweepRandomShots && tries < SweepRandomShots * 20; tries++)
            {
                var p = new Vector2(Mathf.Lerp(min.x, max.x, (float)rng.NextDouble()), Mathf.Lerp(min.y, max.y, (float)rng.NextDouble()));
                if (!InsidePolygon(p, outline) || DistanceToOutline(p, outline) < SweepInset) continue;
                float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
                shots.Add((p, new Vector2(Mathf.Cos(ang), Mathf.Sin(ang))));
                k++;
            }

            var traps = new List<(Vector2 at, int count)>();
            foreach (var (origin, dir) in shots)
            {
                if (Physics2D.OverlapCircleAll(origin + offset, planner.Radius).Any(c => !c.isTrigger && !c.GetComponentInParent<EnemyHealth>())) continue;
                report.SweepShots++;
                var r = planner.Simulate(origin + offset, dir * SweepSpeed, true);
                if (!FindTrap(r, out var at)) continue;
                at -= offset;
                int idx = traps.FindIndex(x => Vector2.Distance(x.at, at) < TrapMergeDistance);
                if (idx < 0) traps.Add((at, 1));
                else traps[idx] = (traps[idx].at, traps[idx].count + 1);
            }

            foreach (var (at, count) in traps)
            {
                string msg = $"ball rattles near {at:0.0} ({count}/{report.SweepShots} sweep shots)";
                if (count >= TrapErrorShots) report.Errors.Add(msg);
                else report.Warnings.Add(msg);
            }
        }

        private static bool FindTrap(LevelLabShotPlanner.Result r, out Vector2 at)
        {
            at = default;
            var path = r.Path;
            var times = r.PathTimes;
            for (int i = 1; i < path.Count - 1; i++)
            {
                int j = i;
                while (j + 1 < path.Count - 1 && times[j + 1] - times[i] <= TrapWindow) j++;
                if (j - i + 1 < TrapContacts) continue;
                var centre = Vector2.zero;
                for (int k = i; k <= j; k++) centre += path[k];
                centre /= j - i + 1;
                bool tight = true;
                for (int k = i; k <= j && tight; k++) tight = Vector2.Distance(path[k], centre) <= TrapRadius;
                if (!tight) continue;
                at = centre;
                return true;
            }

            return false;
        }

        private static void CheckOutlineShape(List<Vector2> outline, Report report)
        {
            float orientation = Mathf.Sign(LevelLabRoomBuilder.SignedArea(outline));
            int n = outline.Count;
            for (int i = 0; i < n; i++)
            {
                var prev = outline[(i - 1 + n) % n];
                var cur = outline[i];
                var next = outline[(i + 1) % n];
                var a = prev - cur;
                var b = next - cur;
                float cross = a.x * b.y - a.y * b.x;
                bool convex = Mathf.Sign(cross) == -orientation || Mathf.Approximately(cross, 0);
                float angle = Vector2.Angle(a, b);
                if (convex && angle < MinOutlineAngle)
                    report.Errors.Add($"outline corner {i} at {cur:0.0} is {angle:0}° (acute wedge traps the ball)");

                for (int j = 0; j < n; j++)
                {
                    if (j == i || (j + 1) % n == i) continue;
                    float d = DistancePointSegment(cur, outline[j], outline[(j + 1) % n]);
                    if (d < MinCorridorWidth)
                        report.Errors.Add($"outline corner {i} at {cur:0.0} is {d:0.0}u from edge {j} (corridor too narrow)");
                }
            }
        }

        private static bool IsRanged(Transform enemy)
        {
            return enemy.name.Contains("Shoot") || enemy.name.Contains("Laser") || enemy.name.Contains("Spread");
        }

        private static bool Blocked(Vector2 from, Vector2 to, List<Collider2D> walls, List<Transform> placed, Transform self)
        {
            var blockers = new List<Collider2D>(walls);
            foreach (var t in placed)
                if (t != self && !t.GetComponentInChildren<EnemyHealth>(true)) blockers.AddRange(SolidColliders(t));
            var offset = (Vector2)TestOffset;
            var hits = Physics2D.LinecastAll(from + offset, to + offset);
            return hits.Any(h => blockers.Contains(h.collider));
        }

        private static List<Collider2D> SolidColliders(Transform t)
        {
            return t.GetComponentsInChildren<Collider2D>(true).Where(c => !c.isTrigger && c.enabled).ToList();
        }

        private static float MinDistance(List<Collider2D> a, List<Collider2D> b)
        {
            float best = float.MaxValue;
            foreach (var ca in a)
            foreach (var cb in b)
            {
                var d = Physics2D.Distance(ca, cb);
                if (d.isValid && d.distance < best) best = d.distance;
            }

            return best;
        }

        public static bool InsidePolygon(Vector2 p, List<Vector2> poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            }

            return inside;
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

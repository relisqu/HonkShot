#if UNITY_EDITOR
using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.LevelLab
{
    // Cheap circle-cast bounce simulation used by the bot to choose throws. It mirrors BouncingObject:
    // reflect on BounceObject surfaces and multiply speed by their bounciness.
    public class LevelLabShotPlanner
    {
        public float Radius = 0.35f;
        public float Damping;
        public float SimStep = 0.02f;
        public float MaxSimTime = 3.5f;
        public float StopSpeed = 1.5f;
        public float TimeFalloff = 0.3f;
        public float RepeatHitWeight = 0.4f;
        public float NonBounceStopFactor = 0f;
        public float PassThroughCooldown = 0.3f;
        public float ProximityWeight = 0.05f;
        public bool IgnoreEnemies;
        public float RoadCaptureDistance = 0.8f;
        public float RoadAcceleration = 60f;
        public float MaxRideSpeed = 40f;
        public float CannonCaptureRadius = 0.6f;
        public float CannonPause = 0.5f;
        public float CannonSpeed = 60f;
        public List<RoadPath> Roads = new();
        public List<CannonAim> Cannons = new();

        public class RoadPath
        {
            public List<Vector2> Points = new();
            public Vector2 ExitDirection;
        }

        public class CannonAim
        {
            public Vector2 Position;
            public Transform Barrel;
        }
        public int MaxContacts = int.MaxValue;
        public static readonly string[] TransientLayers = { "EnemyBullet", "Bullet", "PlayerAttack", "BulletPredictionProjectile", "SimulationGhost" };
        public List<Transform> Targets = new();

        private readonly Transform _ignoreRoot;
        private ContactFilter2D _filter;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
        private readonly Dictionary<Collider2D, (BounceObject bounce, EnemyHealth enemy)> _cache = new();

        public LevelLabShotPlanner(Transform ignoreRoot, int ballLayer)
        {
            _ignoreRoot = ignoreRoot;
            _filter = new ContactFilter2D { useTriggers = true };
            int mask = Physics2D.GetLayerCollisionMask(ballLayer);
            foreach (var transient in TransientLayers)
            {
                int layer = LayerMask.NameToLayer(transient);
                if (layer >= 0) mask &= ~(1 << layer);
            }

            _filter.SetLayerMask(mask);
        }

        public struct Result
        {
            public Vector2 Direction;
            public float Score;
            public int EnemiesHit;
            public List<Vector2> Path;
            public List<float> PathTimes;
            public float MinTargetDistance;
            public int Contacts;
        }

        public Result Best(Vector2 origin, float speed, int samples, bool keepPath = false)
        {
            var best = new Result { Score = -1 };
            for (int i = 0; i < samples; i++)
            {
                float angle = (i + Random.value * 0.5f) * Mathf.PI * 2f / samples;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var r = Simulate(origin, dir * speed, keepPath);
                if (r.Score > best.Score) best = r;
            }

            return best;
        }

        public Result Simulate(Vector2 origin, Vector2 velocity, bool keepPath = false)
        {
            var result = new Result
            {
                Direction = velocity.normalized,
                Path = keepPath ? new List<Vector2> { origin } : null,
                PathTimes = keepPath ? new List<float> { 0f } : null
            };
            var hitCounts = new Dictionary<EnemyHealth, int>();
            var passedThrough = new Dictionary<EnemyHealth, float>();
            var pos = origin;
            var vel = velocity;
            float t = 0;
            Collider2D lastHit = null;
            float minTargetDistance = float.MaxValue;
            var usedRoads = new HashSet<RoadPath>();
            var usedCannons = new HashSet<CannonAim>();

            while (t < MaxSimTime && vel.magnitude > StopSpeed && result.Contacts < MaxContacts)
            {
                float stepDistance = vel.magnitude * SimStep;
                var dir = vel.normalized;
                int count = Physics2D.CircleCast(pos, Radius, dir, _filter, _hits, stepDistance);
                RaycastHit2D hit = default;
                bool found = false;
                for (int i = 0; i < count; i++)
                {
                    var h = _hits[i];
                    if (h.collider == null || h.collider.isTrigger) continue;
                    if (_ignoreRoot && h.collider.transform.IsChildOf(_ignoreRoot)) continue;
                    if (IgnoreEnemies && Lookup(h.collider).enemy) continue;
                    if (h.distance <= 0.0001f && h.collider == lastHit) continue;
                    if (!found || h.distance < hit.distance)
                    {
                        hit = h;
                        found = true;
                    }
                }

                float solidDistance = found ? hit.distance : stepDistance;
                for (int i = 0; i < count; i++)
                {
                    var h = _hits[i];
                    if (h.collider == null || !h.collider.isTrigger || h.distance > solidDistance) continue;
                    var trigger = Lookup(h.collider);
                    if (!trigger.enemy || !trigger.enemy.IsAlive()) continue;
                    if (passedThrough.TryGetValue(trigger.enemy, out float lastTime) && t - lastTime < PassThroughCooldown) continue;
                    passedThrough[trigger.enemy] = t;
                    ScoreEnemyHit(trigger.enemy, t, hitCounts, ref result);
                }

                if (!found)
                {
                    pos += dir * stepDistance;
                }
                else
                {
                    pos = hit.centroid + hit.normal * 0.01f;
                    var info = Lookup(hit.collider);
                    if (info.enemy && info.enemy.IsAlive())
                        ScoreEnemyHit(info.enemy, t, hitCounts, ref result);

                    float bounciness = info.bounce ? info.bounce.Bounciness : NonBounceStopFactor;
                    vel = Vector2.Reflect(vel, hit.normal) * bounciness;
                    lastHit = hit.collider;
                    result.Contacts++;
                    result.Path?.Add(pos);
                    result.PathTimes?.Add(t);
                }

                if (TryRide(ref pos, ref vel, ref t, usedRoads) || TryCannon(ref pos, ref vel, ref t, usedCannons))
                {
                    lastHit = null;
                    result.Path?.Add(pos);
                    result.PathTimes?.Add(t);
                }

                for (int k = 0; k < Targets.Count; k++)
                {
                    if (!Targets[k]) continue;
                    float d = Vector2.Distance(pos, Targets[k].position);
                    if (d < minTargetDistance) minTargetDistance = d;
                }

                vel *= 1f / (1f + SimStep * Damping);
                t += SimStep;
            }

            result.MinTargetDistance = minTargetDistance;
            if (minTargetDistance < float.MaxValue)
                result.Score += ProximityWeight / (1f + minTargetDistance);
            result.Path?.Add(pos);
            result.PathTimes?.Add(t);
            return result;
        }

        private bool TryRide(ref Vector2 pos, ref Vector2 vel, ref float t, HashSet<RoadPath> used)
        {
            foreach (var road in Roads)
            {
                if (used.Contains(road) || road.Points.Count < 2) continue;
                int nearest = -1;
                float best = RoadCaptureDistance;
                for (int i = 0; i < road.Points.Count; i++)
                {
                    float d = Vector2.Distance(pos, road.Points[i]);
                    if (d < best)
                    {
                        best = d;
                        nearest = i;
                    }
                }

                if (nearest < 0 || nearest == road.Points.Count - 1) continue;
                float length = 0;
                for (int i = nearest; i < road.Points.Count - 1; i++)
                    length += Vector2.Distance(road.Points[i], road.Points[i + 1]);
                float speed = Mathf.Max(vel.magnitude, 1f);
                float ride = length / speed;
                pos = road.Points[^1];
                vel = road.ExitDirection * Mathf.Min(speed + RoadAcceleration * ride, MaxRideSpeed);
                t += ride;
                used.Add(road);
                return true;
            }

            return false;
        }

        private bool TryCannon(ref Vector2 pos, ref Vector2 vel, ref float t, HashSet<CannonAim> used)
        {
            foreach (var cannon in Cannons)
            {
                if (used.Contains(cannon) || !cannon.Barrel) continue;
                if (Vector2.Distance(pos, cannon.Position) > CannonCaptureRadius) continue;
                pos = cannon.Position;
                vel = (Vector2)cannon.Barrel.up * CannonSpeed;
                t += CannonPause;
                used.Add(cannon);
                return true;
            }

            return false;
        }

        private void ScoreEnemyHit(EnemyHealth enemy, float t, Dictionary<EnemyHealth, int> hitCounts, ref Result result)
        {
            hitCounts.TryGetValue(enemy, out int n);
            result.Score += (n == 0 ? 1f : RepeatHitWeight) / (1f + TimeFalloff * t);
            if (n == 0) result.EnemiesHit++;
            hitCounts[enemy] = n + 1;
        }

        private (BounceObject bounce, EnemyHealth enemy) Lookup(Collider2D collider)
        {
            if (_cache.TryGetValue(collider, out var info)) return info;
            info = (collider.GetComponent<BounceObject>(), collider.GetComponentInParent<EnemyHealth>());
            _cache[collider] = info;
            return info;
        }
    }
}
#endif

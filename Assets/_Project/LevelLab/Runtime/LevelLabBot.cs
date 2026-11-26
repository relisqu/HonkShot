#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player;
using Scripts.Player.Dash;
using Scripts.Player.InputHandling;
using UnityEngine;

namespace Scripts.LevelLab
{
    // Plays the current room through the real input path (drag start -> aim -> drag release), including
    // mid-flight re-throws, and records how the room plays.
    public class LevelLabBot : MonoBehaviour
    {
        public float AimHoldTime = 0.5f;
        public float ReleaseRetryTime = 3f;
        public int RoadSamples = 24;
        public float AimNoiseDegrees = 5f;
        public float MinThrowInterval = 0.8f;
        public float RestSpeed = 1.2f;
        public float LowSpeed = 4f;
        public float ImproveFactor = 1.6f;
        public float ImproveMargin = 0.35f;
        public int Samples = 64;
        public float ReplanInterval = 0.2f;
        public float Timeout = 90f;
        public float SharpDropMinSpeed = 6f;
        public float SharpDropRatio = 0.4f;
        public float StuckSpeed = 1f;
        public int RattleContacts = 8;
        public float RattleWindow = 1f;
        public float RattleRadius = 2f;
        public float HealBelowPercent = 0.5f;
        public float MaxIdleBetweenThrows = 3f;

        public bool Finished { get; private set; }
        public LevelLabRoomResult Result { get; private set; }

        private Room _room;
        private InputHandler _input;
        private PlayerStatus _status;
        private PlayerDashController _dash;
        private PlayerBallMovement _ballMovement;
        private Rigidbody2D _rigidbody;
        private CircleCollider2D _ballCollider;
        private HealthController _health;
        private LevelLabShotPlanner _planner;

        private bool _throwing;
        private float _lastThrowTime = -10f;
        private float _startReal;
        private float _startGame;
        private float _prevSpeed;
        private float _speedSum;
        private int _speedSamples;
        private readonly List<(float time, Vector2 pos)> _contacts = new();

        public void Begin(Room room, LevelLabRoomResult result)
        {
            _room = room;
            Result = result;
            _status = FindObjectOfType<PlayerStatus>();
            _input = FindObjectOfType<InputHandler>();
            _dash = _status.GetComponentInChildren<PlayerDashController>(true) ?? FindObjectOfType<PlayerDashController>();
            _ballMovement = _status.GetComponentInChildren<PlayerBallMovement>(true) ?? FindObjectOfType<PlayerBallMovement>();
            _rigidbody = FindObjectOfType<PlayerMovement>().GetRigidbody();
            _health = _status.GetHealthController();
            if (!_health) _health = FindObjectOfType<PlayerHealth>().HealthController;

            var bouncing = FindObjectOfType<PlayerBouncingObject>(true);
            var colliderField = typeof(PlayerBouncingObject).GetField("_circleCollider", BindingFlags.NonPublic | BindingFlags.Instance);
            _ballCollider = bouncing ? colliderField?.GetValue(bouncing) as CircleCollider2D : null;
            int ballLayer = _ballCollider ? _ballCollider.gameObject.layer : _rigidbody.gameObject.layer;

            _planner = new LevelLabShotPlanner(_status.transform.root, ballLayer)
            {
                Radius = _ballCollider ? _ballCollider.radius * Mathf.Abs(_ballCollider.transform.lossyScale.x) : 0.35f
            };
            CollectRoadsAndCannons(room);

            result.enemies = room.GetComponentsInChildren<EnemyHealth>(true).Length;
            _health.OnDamageReceived += Health_DamageReceived;
            _ballMovement.OnBounced += BallMovement_Bounced;
            StartCoroutine(Run());
        }

        private void OnDestroy()
        {
            if (_health) _health.OnDamageReceived -= Health_DamageReceived;
            if (_ballMovement) _ballMovement.OnBounced -= BallMovement_Bounced;
        }

        private void Health_DamageReceived(float damage, DamageSource source)
        {
            Result.damageTaken += damage;
            if (_health.RemainingHealthPercentage < HealBelowPercent)
                _health.AddHealth(_health.GetMaxHealth());
        }

        private void BallMovement_Bounced(Collision2D collision)
        {
            if (Finished) return;
            _contacts.Add((Time.time, _rigidbody.position));
            _contacts.RemoveAll(c => Time.time - c.time > RattleWindow);
            if (_contacts.Count < RattleContacts) return;
            var center = _contacts.Aggregate(Vector2.zero, (s, c) => s + c.pos) / _contacts.Count;
            if (_contacts.All(c => Vector2.Distance(c.pos, center) < RattleRadius))
            {
                Result.stuckEvents++;
                _contacts.Clear();
            }
        }

        private IEnumerator Run()
        {
            while (Time.timeScale < 0.95f || !_input.IsInputEnabled)
                yield return null;

            _startReal = Time.realtimeSinceStartup;
            _startGame = Time.time;

            while (true)
            {
                float elapsed = Time.realtimeSinceStartup - _startReal;
                if (!_room || _room.IsCleared())
                {
                    Result.cleared = true;
                    break;
                }

                if (!_health.IsAlive)
                {
                    Result.failure = "player died";
                    break;
                }

                if (elapsed > Timeout)
                {
                    Result.timeout = true;
                    break;
                }

                if (!_throwing && ShouldThrow(out bool midFlight))
                    yield return Throw(midFlight);

                yield return new WaitForSecondsRealtime(ReplanInterval);
            }

            Result.realTime = Time.realtimeSinceStartup - _startReal;
            Result.gameTime = Time.time - _startGame;
            Result.avgBallSpeed = _speedSamples > 0 ? _speedSum / _speedSamples : 0;
            Finished = true;
        }

        private bool ShouldThrow(out bool midFlight)
        {
            midFlight = false;
            if (!_dash.CanDash || Time.realtimeSinceStartup - _lastThrowTime < MinThrowInterval) return false;
            if (_status.PlayerState == PlayerState.Swapping) return false;

            float speed = _rigidbody.linearVelocity.magnitude;
            if (_status.PlayerState != PlayerState.Ball || speed < RestSpeed) return true;

            RefreshTargets();
            if (Time.realtimeSinceStartup - _lastThrowTime > MaxIdleBetweenThrows)
            {
                midFlight = true;
                return true;
            }

            var origin = BallCenter();
            _planner.Damping = _rigidbody.linearDamping;
            float current = _planner.Simulate(origin, _rigidbody.linearVelocity).Score;
            float best = _planner.Best(origin, ThrowSpeed(), Samples / 2).Score;
            midFlight = true;
            if (speed < LowSpeed && best > 0) return true;
            return best > current * ImproveFactor + ImproveMargin;
        }

        private IEnumerator Throw(bool midFlight)
        {
            _throwing = true;
            _input.InputHandler_DragStarted();
            if (_status.PlayerState != PlayerState.Swapping)
            {
                _throwing = false;
                yield break;
            }

            yield return new WaitForSecondsRealtime(AimHoldTime);

            RefreshTargets();
            var origin = BallCenter();
            _planner.Damping = _rigidbody.linearDamping;
            var plan = _planner.Best(origin, ThrowSpeed(), Samples);
            var dir = plan.Score > 0 ? plan.Direction : FallbackDirection(origin);
            dir = Quaternion.Euler(0, 0, Random.Range(-AimNoiseDegrees, AimNoiseDegrees)) * dir;
            _input.InputHandler_DragFinished(-dir * _ballMovement.MaxForceMagnitude);
            float releaseStart = Time.realtimeSinceStartup;
            while (_status.PlayerState == PlayerState.Swapping && Time.realtimeSinceStartup - releaseStart < ReleaseRetryTime)
            {
                // A cannon can swallow the goose mid-aim and drop the release while input is off.
                yield return null;
                if (_input.IsInputEnabled) _input.InputHandler_DragFinished(-dir * _ballMovement.MaxForceMagnitude);
            }

            Result.throws++;
            if (midFlight) Result.midFlightThrows++;
            _lastThrowTime = Time.realtimeSinceStartup;
            _prevSpeed = 0;
            _throwing = false;
        }

        private void CollectRoadsAndCannons(Component room)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            foreach (var road in room.GetComponentsInChildren<Scripts.LevelObjects.MoveRoads.MoveRoadMovement>(true))
            {
                var container = typeof(Scripts.LevelObjects.MoveRoads.MoveRoadMovement).GetField("_splineContainer", flags)?.GetValue(road)
                    as UnityEngine.Splines.SplineContainer;
                if (!container) continue;
                var path = new LevelLabShotPlanner.RoadPath();
                for (int i = 0; i <= RoadSamples; i++)
                {
                    Vector3 p = container.EvaluatePosition(i / (float)RoadSamples);
                    path.Points.Add(p);
                }

                Vector3 tangent = container.EvaluateTangent(1f);
                path.ExitDirection = ((Vector2)tangent).normalized;
                _planner.Roads.Add(path);
            }

            foreach (var cannon in room.GetComponentsInChildren<Scripts.LevelSystem.LevelObjects.Cannon>(true))
            {
                var parts = typeof(Scripts.LevelSystem.LevelObjects.Cannon).GetField("_rotatingParts", flags)?.GetValue(cannon)
                    as List<Transform>;
                var barrel = parts != null && parts.Count > 0 && parts[0] ? parts[0] : cannon.transform;
                _planner.Cannons.Add(new LevelLabShotPlanner.CannonAim { Position = cannon.transform.position, Barrel = barrel });
            }
        }

        private void RefreshTargets()
        {
            _planner.Targets.Clear();
            foreach (var enemy in _room.GetComponentsInChildren<EnemyHealth>())
                if (enemy.IsAlive()) _planner.Targets.Add(enemy.transform);
        }

        private float ThrowSpeed()
        {
            return _ballMovement.GetDragVelocityMagnitude(_rigidbody, Vector2.one.normalized * _ballMovement.MaxForceMagnitude);
        }

        private Vector2 BallCenter()
        {
            return _ballCollider ? (Vector2)_ballCollider.transform.TransformPoint(_ballCollider.offset) : _rigidbody.position;
        }

        private Vector2 FallbackDirection(Vector2 origin)
        {
            var target = _room.GetComponentsInChildren<EnemyHealth>()
                .Where(e => e.IsAlive())
                .OrderBy(e => Vector2.Distance(e.transform.position, origin))
                .FirstOrDefault();
            return target ? ((Vector2)target.transform.position - origin).normalized : Random.insideUnitCircle.normalized;
        }

        private void FixedUpdate()
        {
            if (Finished || Result == null || _throwing || Time.timeScale < 0.5f) return;
            float speed = _rigidbody.linearVelocity.magnitude;

            if (_status.PlayerState == PlayerState.Ball)
            {
                _speedSum += speed;
                _speedSamples++;

                if (_prevSpeed > SharpDropMinSpeed && speed < _prevSpeed * (1f - SharpDropRatio))
                    Result.sharpDrops++;

            }

            if (_status.PlayerState != PlayerState.Swapping && speed < StuckSpeed && !_dash.CanDash)
                Result.deadTime += Time.fixedDeltaTime;

            _prevSpeed = speed;
        }
    }
}
#endif

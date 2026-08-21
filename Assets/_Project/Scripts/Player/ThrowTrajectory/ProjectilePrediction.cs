using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.LevelObjects;
using Scripts.Player.InputHandling;

namespace Scripts.Player
{
    /// <summary>
    /// Shows a predicted trajectory for the player's throw by simulating a duplicate physics scene.
    /// The simulation scene is created once per room and reused for every drag, eliminating expensive
    /// create/unload cycles. Ghost objects mirror their live counterparts each frame; if an original is
    /// destroyed, its ghost is simply deactivated.  Trajectory prediction itself is throttled and stops
    /// after a configurable number of bounces.
    /// </summary>
    public class Projection : MonoBehaviour
    {
        // ─────────────────────────── Inspector ───────────────────────────
        [Header("Trajectory visuals")] [SerializeField]
        private LineRenderer _line;

        [SerializeField] private LineRenderer _raycastLine;

        [SerializeField] private int _maxPhysicsFrameIterations = 100; // renderer vertices (upper‑bound)
        [SerializeField] private int _maxBounces = 2; // stop simulation after N bounces
        [SerializeField] private Vector3 _lineOffset;
        [SerializeField] private int _simSubSteps = 4; // with adaptive stepping this only sets the covered flight LENGTH (maxIter * fixedDt / subSteps), not the step resolution

        [Header("Adaptive stepping")]
        [SerializeField] private int _flightSubSteps = 1;    // coarse steps during open flight (cheap)
        [SerializeField] private int _bounceSubSteps = 8;    // fine steps near walls (accurate bounces)
        [SerializeField] private float _wallProximityMargin = 0.75f; // world units; switch to fine steps within this range of a wall

        [Header("References")] [SerializeField]
        private InputHandler _inputHandler;

        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private PlayerGhostProjectile _playerGhostProjectile;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private float _flightSimInterval = 0.2f;

        // ─────────────────────────── Runtime fields ──────────────────────
        private Scene _simulationScene;
        private PhysicsScene2D _physicsScene;
        private readonly Dictionary<Transform, Transform> _spawnedObjects = new();

        private bool _isDragging;
        private int _simulationGhostLayer;

        // Drag control
        private Vector2 _lastDragDirection;
        private bool _needsRecalculation = true;
        private const float _dragChangeThreshold = 0.1f;

        [SerializeField]
        private float _simulationRefreshInterval = 0.033f; // seconds (unscaled) between trajectory refreshes (~30Hz cap)

        private float _nextSimTime;
        private float _nextFlightSimTime;
        [SerializeField] private float _minFlightSpeedForPrediction = 0.5f;

        // Pooled projectile
        private PlayerGhostProjectile _pooledGhostProjectile;
        private CircleCollider2D _ghostCollider;

        // Adaptive-stepping look-ahead
        private readonly RaycastHit2D[] _castBuf = new RaycastHit2D[4];
        private ContactFilter2D _castFilter;
        private bool _castFilterReady;

        private Room _currentRoom;

        // ─────────────────────────── Unity lifecycle ─────────────────────
        private void OnEnable()
        {
            _simulationGhostLayer = LayerMask.NameToLayer("SimulationGhost");
            _playerMovement.DragStarted += OnDragStarted;
            _playerMovement.DragFinished += OnDragFinished;
            _playerMovement.InputCancelled += PlayerMovement_InputCancelled;
        }

        private void OnDisable()
        {
            _playerMovement.DragStarted -= OnDragStarted;
            _playerMovement.InputCancelled -= PlayerMovement_InputCancelled;
            _playerMovement.DragFinished -= OnDragFinished;
        }

        public static Projection Instance { get; private set; }

        private void Start()
        {
            Instance = this;
            TryCreateSimulationSceneIfNeeded();
            if (LevelManager.Instance)
            {
                Debug.Log("Projection:: LevelManager.Instance.EnteredRoom += LevelManager_EnteredRoom;");
                LevelManager.Instance.EnteredRoom += LevelManager_EnteredRoom;
            }
        }

        public void EnableLine(bool value)
        {
            _line.enabled = value;
        }

        private void OnDestroy()
        {
            if (_simulationScene.IsValid() && _simulationScene.isLoaded)
                SceneManager.UnloadSceneAsync(_simulationScene);

            if (LevelManager.Instance)
                LevelManager.Instance.EnteredRoom -= LevelManager_EnteredRoom;
        }

        public void EnterRoom(Room room)
        {
            DestroySimulation();
            Debug.Log("Projection::EnterRoom");
            _currentRoom = room; // invalidate → rebuilt lazily
            if (_simulationScene.IsValid() && _simulationScene.isLoaded)
                SceneManager.UnloadSceneAsync(_simulationScene);

            _spawnedObjects.Clear();
            _needsRecalculation = true;
            _simulationScene = new Scene();
        }

        // ─────────────────────────── Room change ─────────────────────────
        private void LevelManager_EnteredRoom()
        {
            Debug.Log("Projection::LevelManager_EnteredRoom");
            //  EnterRoom(LevelManager.Instance.CurrentRoom);
        }

        // ─────────────────────────── Drag handlers ───────────────────────
        private void OnDragStarted()
        {
            _isDragging = true;
            _needsRecalculation = true;
            _nextSimTime = 0f; // force an immediate first simulation on the next Update
            TryCreateSimulationSceneIfNeeded();
        }

        /// <summary>Recursively copies position, rotation and scale from src → dst hierarchy.</summary>
        /// <summary>
        /// Recursively copies position/rotation/scale from src → dst hierarchy.
        /// If the source hierarchy lost some children since the clone was made,
        /// the corresponding ghost children are de‑activated so that they no longer
        /// contribute to collisions in the simulation scene.
        /// </summary>
        private static void CopyTransformRecursive(Transform src, Transform dst)
        {
            if (!src)
            {
                if (dst.gameObject.activeSelf) dst.gameObject.SetActive(false);
                return;
            }

            if (!dst.gameObject.activeSelf) dst.gameObject.SetActive(true);

            dst.position = src.position;
            dst.rotation = src.rotation;
            dst.localScale = src.localScale;

            int srcChildren = src.childCount;
            int dstChildren = dst.childCount;
            int common = Mathf.Min(srcChildren, dstChildren);

            // Sync the children that still exist in both hierarchies
            for (int i = 0; i < common; i++)
            {
                CopyTransformRecursive(src.GetChild(i), dst.GetChild(i));
            }

            // Disable ghost children that no longer have a source counterpart
            for (int i = common; i < dstChildren; i++)
            {
                var ghostChild = dst.GetChild(i);
                if (ghostChild.gameObject.activeSelf)
                    ghostChild.gameObject.SetActive(false);
            }
        }

        private void OnDragFinished(Vector2 _)
        {
            _isDragging = false;
            _needsRecalculation = true;
            _line.positionCount = 0;

            if (_pooledGhostProjectile)
                _pooledGhostProjectile.Rigidbody2D.simulated = false;
        }


        private void PlayerMovement_InputCancelled()
        {
            _isDragging = false;
            _needsRecalculation = true;

            if (_pooledGhostProjectile)
                _pooledGhostProjectile.Rigidbody2D.simulated = false;

            _line.positionCount = 0;
            _raycastLine.positionCount = 0;
        }

        // ─────────────────────────── Update loop ─────────────────────────
        private void Update()
        {
            TryCreateSimulationSceneIfNeeded();
            if (!_currentRoom || !_simulationScene.isLoaded) return;

            if (!_isDragging) return;

            // Track the latest aim every frame (cheap). The heavy work — ghost sync + physics simulation —
            // is throttled to _simulationRefreshInterval, so fast aim/camera movement no longer re-runs the
            // full simulation every frame (that was the FPS drop). First sim after drag start is immediate
            // because OnDragStarted resets _nextSimTime to 0.
            _lastDragDirection = _inputHandler.GetCurrentDrag();

            if (Time.unscaledTime >= _nextSimTime)
            {
                SyncGhosts();
                SimulateTrajectory();
                _nextSimTime = Time.unscaledTime + _simulationRefreshInterval;
            }
        }

        // Mirror live obstacles onto their simulation ghosts. Only needed right before a simulation, so it
        // runs at the throttled recalc rate rather than every frame.
        private void SyncGhosts()
        {
            foreach (var pair in _spawnedObjects)
            {
                if (!pair.Key)
                {
                    if (pair.Value) pair.Value.gameObject.SetActive(false);
                }
                else if (pair.Value)
                {
                    CopyTransformRecursive(pair.Key, pair.Value);
                }
            }
        }

        // ─────────────────────────── Scene setup ─────────────────────────
        private void TryCreateSimulationSceneIfNeeded()
        {
            if (_simulationScene.IsValid()) return;
            if (!_currentRoom) return;

            Debug.Log("Projection::Trying to create simulation scene");
            _simulationScene =
                SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            _physicsScene = _simulationScene.GetPhysicsScene2D();

            foreach (Transform obj in _currentRoom.ObstaclesTransform)
                if (ShouldBeCloned(obj))
                    CloneHierarchy(obj, _simulationScene);
        }

        private void CloneHierarchy(Transform source, Scene targetScene)
        {
            var ghost = Instantiate(source.gameObject, source.position, source.rotation);
            DisableBehavioursRecursive(ghost.transform);
            DestroyTriggerColliders(source, ghost.transform);
            SetLayerRecursively(ghost.transform, _simulationGhostLayer);
            SceneManager.MoveGameObjectToScene(ghost, targetScene);
            _spawnedObjects[source] = ghost.transform;
        }

        private static void DestroyTriggerColliders(Transform source, Transform ghost)
        {
            var srcColliders = source.GetComponentsInChildren<Collider2D>(true);
            var dstColliders = ghost.GetComponentsInChildren<Collider2D>(true);
            int count = Mathf.Min(srcColliders.Length, dstColliders.Length);
            for (int i = 0; i < count; i++)
            {
                if (srcColliders[i].isTrigger)
                    Destroy(dstColliders[i]);
            }
        }

        private void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
                SetLayerRecursively(root.GetChild(i), layer);
        }


        private static readonly string[] _visualPrefixes = { "visual", "canvas", "shadow", "particle" };

        private bool ShouldBeCloned(Transform t)
        {
            if (_spawnedObjects.ContainsKey(t)) return false;
            string name = t.name.ToLower().Trim();
            foreach (var p in _visualPrefixes)
                if (name.Contains(p))
                    return false;
            return t.gameObject.activeInHierarchy;
        }


        /// <summary>
        /// Disables all MonoBehaviour components on the ghost clone so that AI, audio, and damage scripts do not
        /// affect gameplay, while keeping colliders & rigidbodies active for physics‑only simulation.
        /// Also disables Canvas components so ghost HP bars don't render alongside the real ones.
        /// </summary>
        private static void DisableBehavioursRecursive(Transform root)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                mb.enabled = false;
            }
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                canvas.enabled = false;
            }
        }

        // ─────────────────────────── Trajectory sim ──────────────────────
        private void SimulateTrajectory()
        {
            _needsRecalculation = false;

            // lazy‑create pooled projectile once
            if (!_pooledGhostProjectile)
            {
                _pooledGhostProjectile = Instantiate(_playerGhostProjectile);
                _pooledGhostProjectile.Rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                _ghostCollider = _pooledGhostProjectile.GetComponent<CircleCollider2D>();
                SetLayerRecursively(_pooledGhostProjectile.transform, _simulationGhostLayer);
                SceneManager.MoveGameObjectToScene(_pooledGhostProjectile.gameObject, _simulationScene);
                _pooledGhostProjectile.gameObject.SetActive(false);
            }

            var proj = _pooledGhostProjectile;
            proj.gameObject.SetActive(true);
            proj.transform.position = _playerBallMovement.transform.position;
            proj.transform.rotation = Quaternion.identity;

            var rb = proj.Rigidbody2D;
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            _playerBallMovement.ThrowRigidbody(rb, _lastDragDirection);
            proj.BouncingObject.SetCurrentVelocity(rb.linearVelocity);
            proj.BouncingObject.ClearDebugData();


            // Adaptive stepping: big cheap steps in open flight, fine steps only when a wall is within
            // reach — bounces stay as accurate as small fixed steps but open flight costs a fraction.
            // Total covered flight time is kept equal to the old fixed-step length so the line is the
            // same length as before (maxIter * fixedDt / simSubSteps).
            if (!_castFilterReady)
            {
                _castFilter = new ContactFilter2D { useTriggers = false };
                _castFilter.SetLayerMask(1 << _simulationGhostLayer);
                _castFilterReady = true;
            }

            float fixedDt = Time.fixedUnscaledDeltaTime;
            float coarseDt = fixedDt / Mathf.Max(1, _flightSubSteps);
            float fineDt = fixedDt / Mathf.Max(1, _bounceSubSteps);
            float maxFlightTime = _maxPhysicsFrameIterations * (fixedDt / Mathf.Max(1, _simSubSteps));
            float worldRadius = _ghostCollider.radius * Mathf.Abs(proj.transform.lossyScale.x);

            int idx = 0;
            int bounceCount = 0;
            float simTime = 0f;
            Vector2 prevVelocity = rb.linearVelocity;

            _line.positionCount = _maxPhysicsFrameIterations;
            _line.SetPosition(idx++, proj.transform.position + _lineOffset);

            while (idx < _maxPhysicsFrameIterations && bounceCount <= _maxBounces && simTime < maxFlightTime)
            {
                Vector2 pos = proj.transform.position;
                Vector2 vel = rb.linearVelocity;
                float speed = vel.magnitude;
                if (speed < 0.0001f) break;
                Vector2 dir = vel / speed;

                // Look ahead one coarse step (+margin); if a wall is that close, step finely through it.
                float lookDist = coarseDt * speed + _wallProximityMargin;
                bool wallAhead = false;
                int hits = _physicsScene.CircleCast(pos, worldRadius, dir, lookDist, _castFilter, _castBuf);
                for (int i = 0; i < hits; i++)
                {
                    var hitCol = _castBuf[i].collider;
                    if (hitCol && hitCol != _ghostCollider) { wallAhead = true; break; }
                }

                float dt = wallAhead ? fineDt : coarseDt;
                _physicsScene.Simulate(dt);
                simTime += dt;

                proj.BouncingObject.SetCurrentVelocity(rb.linearVelocity);
                _line.SetPosition(idx++, proj.transform.position + _lineOffset);

                if (prevVelocity.sqrMagnitude > 0.0001f
                    && rb.linearVelocity.sqrMagnitude > 0.0001f
                    && Vector2.Angle(prevVelocity, rb.linearVelocity) > 5f)
                {
                    bounceCount++;
                }

                prevVelocity = rb.linearVelocity;
            }

            _line.positionCount = idx;

            rb.simulated = false;
        }

        private void SimulateFromCurrentState()
        {
            if (!_pooledGhostProjectile) return;

            SyncGhostRigidbodies();

            var proj = _pooledGhostProjectile;
            proj.gameObject.SetActive(true);
            proj.transform.position = _playerBallMovement.transform.position;
            proj.transform.rotation = _playerBallMovement.transform.rotation;

            var rb = proj.Rigidbody2D;
            rb.simulated = true;
            rb.linearVelocity = _playerBallMovement.CurrentMovement;
            rb.angularVelocity = 0f;

            proj.BouncingObject.SetCurrentVelocity(rb.linearVelocity);

            float simDt = Time.fixedUnscaledDeltaTime / _simSubSteps;
            int idx = 0;
            int bounceCount = 0;
            Vector2 prevVelocity = rb.linearVelocity;

            _line.positionCount = _maxPhysicsFrameIterations;
            _line.SetPosition(idx++, proj.transform.position + _lineOffset);

            while (idx < _maxPhysicsFrameIterations && bounceCount <= _maxBounces)
            {
                _physicsScene.Simulate(simDt);
                proj.BouncingObject.SetCurrentVelocity(rb.linearVelocity);
                _line.SetPosition(idx++, proj.transform.position + _lineOffset);

                if (prevVelocity.sqrMagnitude > 0.0001f
                    && rb.linearVelocity.sqrMagnitude > 0.0001f
                    && Vector2.Angle(prevVelocity, rb.linearVelocity) > 5f)
                {
                    bounceCount++;
                }

                prevVelocity = rb.linearVelocity;
            }

            _line.positionCount = idx;
            rb.simulated = false;
        }

        private void SyncGhostRigidbodies()
        {
            foreach (var pair in _spawnedObjects)
            {
                if (!pair.Key || !pair.Value) continue;

                var srcRb = pair.Key.GetComponent<Rigidbody2D>();
                var dstRb = pair.Value.GetComponent<Rigidbody2D>();

                if (srcRb && dstRb)
                {
                    dstRb.linearVelocity = srcRb.linearVelocity;
                    dstRb.angularVelocity = srcRb.angularVelocity;
                }
            }
        }

        public bool IsSimulationReady()
        {
            return _simulationScene.IsValid() && _simulationScene.isLoaded;
        }

        public void DestroySimulation()
        {
            if (_simulationScene.IsValid() && _simulationScene.isLoaded)
                SceneManager.UnloadSceneAsync(_simulationScene);
        }
    }
}
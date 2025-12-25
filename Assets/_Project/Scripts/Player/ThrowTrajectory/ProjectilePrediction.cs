using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.LevelSystem.LevelGeneration;
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

        [SerializeField] private int _maxPhysicsFrameIterations = 100; // renderer vertices (upper‑bound)
        [SerializeField] private float _simulationStepMultiplier = 2f; // dt multiplier for faster sim
        [SerializeField] private int _maxBounces = 2; // stop simulation after N bounces

        [Header("References")] [SerializeField]
        private InputHandler _inputHandler;

        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private PlayerGhostProjectile _playerGhostProjectile;

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
        private float _simulationRefreshInterval = 0.05f; // seconds (unscaled) between trajectory refreshes

        private float _nextSimTime;

        // Pooled projectile
        private PlayerGhostProjectile _pooledGhostProjectile;

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

            if (_pooledGhostProjectile)
                _pooledGhostProjectile.gameObject.SetActive(false);

            // hide line but keep simulation scene
            _line.positionCount = 0;
        }


        private void PlayerMovement_InputCancelled()
        {
            _isDragging = false;
            _needsRecalculation = true;

            if (_pooledGhostProjectile)
                _pooledGhostProjectile.gameObject.SetActive(false);

            // hide line but keep simulation scene
            _line.positionCount = 0;
        }

        // ─────────────────────────── Update loop ─────────────────────────
        private void Update()
        {
            TryCreateSimulationSceneIfNeeded();
            if (!_currentRoom || !_simulationScene.isLoaded) return;

            // Sync ghosts → originals
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

            if (_isDragging)
            {
                Vector2 currentDrag = _inputHandler.GetCurrentDrag();
                if (Vector2.Distance(currentDrag, _lastDragDirection) > _dragChangeThreshold)
                {
                    _lastDragDirection = currentDrag;
                    _needsRecalculation = true;
                }

                if (_needsRecalculation || Time.unscaledTime >= _nextSimTime)
                {
                    SimulateTrajectory();
                    _nextSimTime = Time.unscaledTime + _simulationRefreshInterval;
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
            SetLayerRecursively(ghost.transform, _simulationGhostLayer);
            SceneManager.MoveGameObjectToScene(ghost, targetScene);
            _spawnedObjects[source] = ghost.transform;
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
        /// </summary>
        private static void DisableBehavioursRecursive(Transform root)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(false))
            {
                // keep this script (Projection) or other whitelisted ones if ever needed
                mb.enabled = false;
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

            // simulation variables
            float simDt = Time.fixedUnscaledDeltaTime;
            int idx = 0;
            int bounceCount = 0;
            Vector2 prevVelocity = rb.linearVelocity;

            _line.positionCount = _maxPhysicsFrameIterations;
            _line.SetPosition(idx++, proj.transform.position);

            while (idx < _maxPhysicsFrameIterations && bounceCount <= _maxBounces)
            {
                _physicsScene.Simulate(simDt);
                proj.BouncingObject.SetCurrentVelocity(rb.linearVelocity);
                _line.SetPosition(idx++, proj.transform.position);
                if (rb.linearVelocity.sqrMagnitude > 0.0001f && Vector2.Angle(prevVelocity, rb.linearVelocity) > 5f)
                {
                    bounceCount++;
                }

                prevVelocity = rb.linearVelocity;
            }

            _line.positionCount = idx;

            rb.simulated = false;
            proj.gameObject.SetActive(false);
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
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
        private int _frameCounter;
        private const int _framesBetweenSimulations = 3;

        // Pooled projectile
        private PlayerGhostProjectile _pooledGhostProjectile;

        // ─────────────────────────── Unity lifecycle ─────────────────────
        private void OnEnable()
        {
            _simulationGhostLayer = LayerMask.NameToLayer("SimulationGhost");
            _playerMovement.DragStarted += OnDragStarted;
            _playerMovement.DragFinished += OnDragFinished;

            if (LevelManager.Instance)
                LevelManager.Instance.EnteredRoom += OnRoomEntered;
        }

        private void OnDisable()
        {
            _playerMovement.DragStarted -= OnDragStarted;
            _playerMovement.DragFinished -= OnDragFinished;

            if (LevelManager.Instance)
                LevelManager.Instance.EnteredRoom -= OnRoomEntered;
        }

        private void Start() => TryCreateSimulationSceneIfNeeded();

        private void OnDestroy()
        {
            if (_simulationScene.IsValid() && _simulationScene.isLoaded)
                SceneManager.UnloadSceneAsync(_simulationScene);

            if (LevelManager.Instance)
                LevelManager.Instance.EnteredRoom -= OnRoomEntered;
        }

        // ─────────────────────────── Room change ─────────────────────────
        private void OnRoomEntered()
        {
            if (_simulationScene.IsValid() && _simulationScene.isLoaded)
                SceneManager.UnloadSceneAsync(_simulationScene);

            _spawnedObjects.Clear();
            _needsRecalculation = true;
            _simulationScene = new Scene(); // invalidate → rebuilt lazily
        }

        // ─────────────────────────── Drag handlers ───────────────────────
        private void OnDragStarted()
        {
            _isDragging = true;
            _needsRecalculation = true;
            TryCreateSimulationSceneIfNeeded();
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

        // ─────────────────────────── Update loop ─────────────────────────
        private void Update()
        {
            TryCreateSimulationSceneIfNeeded();
            if (!LevelManager.Instance.CurrentRoom || !_simulationScene.isLoaded) return;

            // Sync ghosts → originals
            foreach (var pair in _spawnedObjects)
            {
                if (!pair.Key)
                {
                    if (pair.Value) pair.Value.gameObject.SetActive(false);
                }
                else if (pair.Value)
                {
                    pair.Value.gameObject.SetActive(true);
                    pair.Value.position = pair.Key.position;
                    pair.Value.rotation = pair.Key.rotation;
                }
            }

            if (_isDragging)
            {
                _frameCounter++;

                Vector2 currentDrag = _inputHandler.GetCurrentDrag();
                if (Vector2.Distance(currentDrag, _lastDragDirection) > _dragChangeThreshold)
                {
                    _lastDragDirection = currentDrag;
                    _needsRecalculation = true;
                }

                if (_needsRecalculation || _frameCounter % _framesBetweenSimulations == 0)
                    SimulateTrajectory();
            }
        }

        // ─────────────────────────── Scene setup ─────────────────────────
        private void TryCreateSimulationSceneIfNeeded()
        {
            if (_simulationScene.IsValid() && _simulationScene.isLoaded) return;
            if (!LevelManager.Instance.CurrentRoom) return;

            _simulationScene =
                SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            _physicsScene = _simulationScene.GetPhysicsScene2D();

            foreach (Transform obj in LevelManager.Instance.CurrentRoom.ObstaclesTransform)
                CloneHierarchy(obj, _simulationScene);
        }

        private void CloneHierarchy(Transform source, Scene targetScene)
        {
            var ghost = Instantiate(source.gameObject, source.position, source.rotation);
            SetLayerRecursively(ghost.transform, _simulationGhostLayer);
            SceneManager.MoveGameObjectToScene(ghost, targetScene);
            _spawnedObjects[source] = ghost.transform;

            for (int i = 0; i < source.childCount; i++)
            {
                var child = source.GetChild(i);
                if (ShouldBeCloned(child)) CloneHierarchy(child, targetScene);
            }
        }

        private void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
                SetLayerRecursively(root.GetChild(i), layer);
        }

        private static readonly string[] _visualPrefixes = { "visual", "canvas" };

        private bool ShouldBeCloned(Transform t)
        {
            string name = t.name.ToLower().Trim();
            foreach (var p in _visualPrefixes)
                if (name.StartsWith(p))
                    return false;
            return t.GetComponent<Collider2D>();
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
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            _playerBallMovement.ThrowRigidbody(rb, _lastDragDirection);

            // simulation variables
            float simDt = Time.fixedUnscaledDeltaTime * _simulationStepMultiplier;
            int idx = 0;
            int bounceCount = 0;
            Vector2 prevVelocity = rb.velocity;

            _line.positionCount = _maxPhysicsFrameIterations;
            _line.SetPosition(idx++, proj.transform.position);

            while (idx < _maxPhysicsFrameIterations && bounceCount <= _maxBounces)
            {
                _physicsScene.Simulate(simDt);
                proj.BouncingObject.SetCurrentVelocity(rb.velocity);

                _line.SetPosition(idx++, proj.transform.position);
                if (rb.velocity.sqrMagnitude > 0.0001f && Vector2.Angle(prevVelocity, rb.velocity) > 5f)
                {
                    bounceCount++;
                    Debug.Log(bounceCount + " " + idx);
                }

                prevVelocity = rb.velocity;
            }

            _line.positionCount = idx;

            rb.simulated = false;
            proj.gameObject.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.InputHandling;

namespace Scripts.Player
{
    public class Projection : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField] private int _maxPhysicsFrameIterations = 100;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private PlayerGhostProjectile _playerGhostProjectile;

        private Scene _simulationScene;
        private PhysicsScene2D _physicsScene;
        private readonly Dictionary<Transform, Transform> _spawnedObjects = new();

        private bool _isDragging;
        private int _simulationGhostLayer;

        private Vector2 _lastDragDirection;
        private bool _needsRecalculation = true;
        private const float _dragChangeThreshold = 0.1f;

        private PlayerGhostProjectile _pooledGhostProjectile;

        private void OnEnable()
        {
            _simulationGhostLayer = LayerMask.NameToLayer("SimulationGhost");
            _playerMovement.DragStarted += OnDragStarted;
            _playerMovement.DragFinished += OnDragFinished;
        }

        private void OnDisable()
        {
            _playerMovement.DragStarted -= OnDragStarted;
            _playerMovement.DragFinished -= OnDragFinished;
        }

        private void OnDragStarted()
        {
            _isDragging = true;
            _needsRecalculation = true;
            CreatePhysicsScene();
        }

        private void OnDragFinished(Vector2 drag)
        {
            _isDragging = false;
            _needsRecalculation = true;

            if (_pooledGhostProjectile != null)
            {
                _pooledGhostProjectile.gameObject.SetActive(false);
            }

            if (_simulationScene.isLoaded)
            {
                _spawnedObjects.Clear();
                SceneManager.UnloadSceneAsync(_simulationScene);
                _line.positionCount = 0;
            }
        }

        private void Update()
        {
            if (!LevelManager.Instance.CurrentRoom) return;

            foreach (var pair in _spawnedObjects)
            {
                if (pair.Key && pair.Value)
                {
                    pair.Value.position = pair.Key.position;
                    pair.Value.rotation = pair.Key.rotation;
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

                if (_needsRecalculation)
                {
                    SimulateTrajectory();
                }
            }
        }

        private void CreatePhysicsScene()
        {
            if (LevelManager.Instance.CurrentRoom == null) return;
            if (SceneManager.GetSceneByName("Simulation").IsValid()) return;

            _simulationScene =
                SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            _physicsScene = _simulationScene.GetPhysicsScene2D();

            foreach (Transform obj in LevelManager.Instance.CurrentRoom.ObstaclesTransform)
            {
                if (IsGhostRelevant(obj))
                {
                    CloneHierarchy(obj, _simulationScene);
                }
            }
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
                if (IsGhostRelevant(child))
                    CloneHierarchy(child, targetScene);
            }
        }

        private bool IsGhostRelevant(Transform obj)
        {
            return !obj.gameObject.isStatic && obj.GetComponent<Collider2D>() != null;
        }

        private void SimulateTrajectory()
        {
            _needsRecalculation = false;

            if (!_pooledGhostProjectile)
            {
                _pooledGhostProjectile = Instantiate(_playerGhostProjectile);
                SetLayerRecursively(_pooledGhostProjectile.transform, _simulationGhostLayer);
                SceneManager.MoveGameObjectToScene(_pooledGhostProjectile.gameObject, _simulationScene);
                _pooledGhostProjectile.gameObject.SetActive(false);
            }

            var projectile = _pooledGhostProjectile;
            projectile.gameObject.SetActive(true);
            projectile.transform.position = _playerBallMovement.transform.position;
            projectile.transform.rotation = Quaternion.identity;

            var rb = projectile.Rigidbody2D;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            _playerBallMovement.ThrowRigidbody(rb, _lastDragDirection);
            projectile.BouncingObject.SetCurrentVelocity(rb.velocity);

            _line.positionCount = _maxPhysicsFrameIterations;
            for (int i = 0; i < _maxPhysicsFrameIterations; i++)
            {
                _physicsScene.Simulate(Time.fixedUnscaledDeltaTime);
                projectile.BouncingObject.SetCurrentVelocity(rb.velocity);
                _line.SetPosition(i, projectile.transform.position);
            }

            projectile.gameObject.SetActive(false);
        }

        private void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layer);
            }
        }
    }
}
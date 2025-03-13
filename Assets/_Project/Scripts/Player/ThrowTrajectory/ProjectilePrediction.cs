using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Player
{
    public class Projection : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField] private int _maxPhysicsFrameIterations = 100;
        [SerializeField] private Transform _obstaclesParent;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private PlayerGhostProjectile _playerGhostProjectile;

        private Scene _simulationScene;
        private PhysicsScene2D _physicsScene;
        private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();

        private bool _isDragging;

        private void OnEnable()
        {
            _inputHandler.OnDragStarted += InputHandler_DragStarted;
            _inputHandler.OnDragFinished += InputHandler_DragFinished;
        }

        private void OnDisable()
        {
            _inputHandler.OnDragStarted -= InputHandler_DragStarted;
            _inputHandler.OnDragFinished -= InputHandler_DragFinished;
        }

        private void InputHandler_DragStarted()
        {
            _isDragging = true;
            CreatePhysicsScene();
        }

        private void InputHandler_DragFinished(Vector2 drag)
        {
            _isDragging = false;
            if (_simulationScene.isLoaded)
            {
                _spawnedObjects.Clear();
                SceneManager.UnloadSceneAsync(_simulationScene);
                _line.positionCount = 0;
            }
        }


        private void Start()
        {
        }

        private void CreatePhysicsScene()
        {
            _simulationScene =
                SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            _physicsScene = _simulationScene.GetPhysicsScene2D();

            foreach (Transform obj in _obstaclesParent)
            {
                var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
                var spriteRenderer = ghostObj.GetComponentInChildren<SpriteRenderer>(false);
                if (spriteRenderer!=null)
                {
                    spriteRenderer.enabled = false;
                }

                SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
                if (!ghostObj.isStatic) _spawnedObjects.Add(obj, ghostObj.transform);
            }
        }

        private void Update()
        {
            foreach (var item in _spawnedObjects)
            {
                item.Value.position = item.Key.position;
                item.Value.rotation = item.Key.rotation;
            }

            if (_isDragging)
                SimulateTrajectory();
        }

        private PlayerGhostProjectile _playerGhostProjectileObject;

        public void SimulateTrajectory()
        {
            if (_playerGhostProjectileObject != null) return;
            _playerGhostProjectileObject = Instantiate(_playerGhostProjectile, _playerBallMovement.transform.position,
                Quaternion.identity);
            SceneManager.MoveGameObjectToScene(_playerGhostProjectileObject.gameObject, _simulationScene);
            _playerBallMovement.ThrowRigidbody(_playerGhostProjectileObject.Rigidbody2D,
                _inputHandler.GetCurrentDrag());
            _line.positionCount = _maxPhysicsFrameIterations;
            _playerGhostProjectileObject.BouncingObject.SetCurrentVelocity(_playerGhostProjectileObject.Rigidbody2D
                .velocity);

            for (var i = 0; i < _maxPhysicsFrameIterations; i++)
            {
                _physicsScene.Simulate(Time.fixedUnscaledDeltaTime);
                _playerGhostProjectileObject.BouncingObject.SetCurrentVelocity(_playerGhostProjectileObject.Rigidbody2D
                    .velocity);
                _line.SetPosition(i, _playerGhostProjectileObject.transform.position);
            }

            Destroy(_playerGhostProjectileObject.gameObject);
        }
    }
}
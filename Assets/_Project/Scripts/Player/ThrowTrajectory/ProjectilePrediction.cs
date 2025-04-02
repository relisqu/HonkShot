using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Scripts.Enemies;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();

        private bool _isDragging;

        private void OnEnable()
        {
            _playerMovement.DragStarted += InputHandler_DragStarted;
            _playerMovement.DragFinished += InputHandler_DragFinished;
        }

        private void OnDisable()
        {
            _playerMovement.DragStarted -= InputHandler_DragStarted;
            _playerMovement.DragFinished -= InputHandler_DragFinished;
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
            if(LevelManager.Instance.CurrentRoom== null) return;
            if(SceneManager.GetSceneByName("Simulation").IsValid()) return;
            _simulationScene =SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            _physicsScene = _simulationScene.GetPhysicsScene2D();

            foreach (Transform obj in LevelManager.Instance.CurrentRoom.ObstaclesTransform)
            {
                var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);


                foreach (Transform innerObj in obj)
                {
                    if (innerObj.TryGetComponent(out EnemyHealth enemyHealth))
                    {
                        if (ghostObj.transform != innerObj.transform)
                        {
                            var ghostInnerObj = Instantiate(innerObj.gameObject, innerObj.position, innerObj.rotation);
                            var spriteInnerRenderers = ghostInnerObj.GetComponentsInChildren<SpriteRenderer>(true);
                            foreach (var spriteRenderer in spriteInnerRenderers)
                            {
                                spriteRenderer.enabled = false;
                            }

                            SceneManager.MoveGameObjectToScene(ghostInnerObj, _simulationScene);
                            if (!ghostInnerObj.gameObject.isStatic) _spawnedObjects.Add(innerObj, ghostInnerObj.transform);
                        }
                    }
                }

                var spriteRenderers = ghostObj.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.enabled = false;
                }

                SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
                if (!ghostObj.isStatic)
                {
                    _spawnedObjects.Add(obj, ghostObj.transform);
                }
            }
        }

        private void Update()
        {
            if(!LevelManager.Instance.CurrentRoom) return;
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
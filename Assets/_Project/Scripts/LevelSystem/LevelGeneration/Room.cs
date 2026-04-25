using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.LevelCreator;
using Scripts.Enemies;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class Room : MonoBehaviour
    {
        
        [SerializeField] private Door _door;
        [SerializeField] private Transform _spawnPointTransform;
        [SerializeField] private SpriteShapeController _spriteShapeController;

        [FormerlySerializedAs("_portalSpawnPoint")] [SerializeField]
        private Transform exitPortalSpawnPoint;

        [SerializeField] public Transform ObstaclesTransform;
        private bool _isActive = false;

        private List<EnemyHealth> _enemyHealths = new List<EnemyHealth>();
        public SpriteShapeController SpriteShapeController => _spriteShapeController;
        public Transform SpawnPointTransform => _spawnPointTransform;

        public Action<Room> CompletedRoom;

        public Transform ExitPortalSpawnPoint
        {
            get => exitPortalSpawnPoint;
            set => exitPortalSpawnPoint = value;
        }

        private void Awake()
        {
            UpdateEnemyInfo();
        }

        public void UpdateEnemyInfo()
        {
            var enemies = GetComponentsInChildren<EnemyHealth>();
            _enemyHealths = enemies.ToList();
            Debug.Log($"[Room {gameObject.name}] UpdateEnemyInfo found {_enemyHealths.Count} EnemyHealth components.");
            foreach (var enemyHealth in _enemyHealths)
            {
                Debug.Log($"[Room {gameObject.name}] tracking ENEMY '{enemyHealth.gameObject.name}' alive={enemyHealth.IsAlive()}");
                enemyHealth.HealthController.OnDied += EnemyHealthController_Died;
            }

            if (IsCleared())
            {
                Debug.Log($"[Room {gameObject.name}] UNLOCKED on Awake (no enemies present).");
                UnlockRoom();
            }
        }

        private void EnemyHealthController_Died()
        {
            int aliveCount = _enemyHealths.Count(eh => eh && eh.IsAlive());
            Debug.Log($"[Room {gameObject.name}] EnemyHealthController_Died fired. Alive remaining: {aliveCount}/{_enemyHealths.Count}");
            if (IsCleared())
            {
                UnlockRoom();
            }
        }

        public void UnlockRoom()
        {
            Debug.Log($"Room {gameObject.name} unlocked!");
            _door.Open();
            CompletedRoom?.Invoke(this);
        }

        public bool IsCleared()
        {
            return !_enemyHealths.Any(enemyHealth => enemyHealth.IsAlive());
        }

        public void SetActive()
        {
            gameObject.SetActive(true);
        }

        public void SetNextRoom(Room roomInstance)
        {
            _door.SetRoom(roomInstance);
        }

        public void ExitRoom()
        {
            DestroyRoom();
        }

        public void DestroyRoom()
        {
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (_spawnPointTransform != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(_spawnPointTransform.position, 0.3f);
            }

            if (exitPortalSpawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(exitPortalSpawnPoint.position, 0.4f);
            }
        }

        public bool IsActive() => _isActive;
    }
}
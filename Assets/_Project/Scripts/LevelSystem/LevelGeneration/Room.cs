using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.LevelCreator;
using Scripts.Enemies;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private Door _door;
        [SerializeField] private Transform _spawnPointTransform;
        [SerializeField] public Transform ObstaclesTransform;
        private bool _isActive = false;

        private List<EnemyHealth> _enemyHealths = new List<EnemyHealth>();
        public Transform SpawnPointTransform => _spawnPointTransform;

        private void Start()
        {
            var enemies = GetComponentsInChildren<EnemyHealth>();
            _enemyHealths = enemies.ToList();
            foreach (var enemyHealth in _enemyHealths)
            {
                Debug.Log("ENEMY " + enemyHealth.IsAlive());
                enemyHealth.HealthController.OnDied += EnemyHealthController_Died;
            }

            if (IsCleared())
            {
                Debug.Log("UNLOCKED");
                UnlockRoom();
            }
        }

        private void EnemyHealthController_Died()
        {
            if (IsCleared())
            {
                UnlockRoom();
            }
        }

        public void UnlockRoom()
        {
            Debug.Log($"Room {gameObject.name} unlocked!");
            _door.Open();
            LevelManager.Instance.CompletedRoom?.Invoke();
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
    }
}
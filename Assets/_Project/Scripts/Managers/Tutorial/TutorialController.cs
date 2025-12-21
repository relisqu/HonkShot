using System.Collections;
using System.Collections.Generic;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.TechnicalScripts;
using Scripts.Player;
using Scripts.Progress;
using TMPro;
using UnityEngine;
using Zenject;

namespace Scripts.Managers.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        private LevelTransitionManager _levelTransitionManager;

        [SerializeField] private List<Room> _tutorialRoomPrefabs;
        [SerializeField] private Transform _tutorialRoomParent;
        [SerializeField] private BackgroundPropSpawner _backgroundPropSpawner;
        [SerializeField] private Projection _projection;
        [SerializeField] private PlayerMovement _playerMovement;

        private List<Room> _tutorialRooms = new List<Room>();

        private Room _currentRoom;

        [Inject]
        private void Construct(LevelTransitionManager levelTransitionManager)
        {
            _levelTransitionManager = levelTransitionManager;
        }

        public void CreateTutorial()
        {
            foreach (var room in _tutorialRoomPrefabs)
            {
                var newRoom = Instantiate(room, _tutorialRoomParent);
                newRoom.gameObject.SetActive(false);
                _tutorialRooms.Add(newRoom);
            }
        }

        public void StartTutorial()
        {
            EnterRoom(_tutorialRooms[0]);
        }

        public void EnterRoom(Room room)
        {
            Debug.Log($"Trying to enter room {room}");
            if (!room) return;
            _currentRoom = room;
            _projection.EnterRoom(room);
            _levelTransitionManager.PrepareRoom(room);
            room.SetActive();
            _backgroundPropSpawner.SetLevel(room.SpriteShapeController);
            _playerMovement.SetPosition(room.SpawnPointTransform.position);

            StartCoroutine(StartLevelCoroutineRoutine());
        }

        private IEnumerator StartLevelCoroutineRoutine()
        {
            TimeManager.Instance.SlowGame(0f);
            if (!_currentRoom.ExitPortalSpawnPoint)
            {
                _currentRoom.ExitPortalSpawnPoint = _currentRoom.SpawnPointTransform;
            }

            yield return _levelTransitionManager.SpawnEnterPortal(_currentRoom);

            TimeManager.Instance.ResumeGame();
            yield return null;
        }
    }
}
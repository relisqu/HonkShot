using System.Collections;
using System.Collections.Generic;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.TechnicalScripts;
using Scripts.Player;
using Scripts.Player.InputHandling;
using Scripts.Progress;
using TMPro;
using UnityEngine;
using Zenject;

namespace Scripts.Managers.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        private LevelTransitionManager _levelTransitionManager;
        private LevelManager _levelManager;
        private ProgressSaver _progressSaver;
        private DiContainer _diContainer;
        private InputHandler _inputHandler;

        [SerializeField] private List<TutorialRoom> _tutorialRoomPrefabs;
        [SerializeField] private Transform _tutorialRoomParent;
        [SerializeField] private BackgroundPropSpawner _backgroundPropSpawner;
        [SerializeField] private Projection _projection;
        [SerializeField] private PlayerMovement _playerMovement;

        private List<TutorialRoom> _tutorialRooms = new List<TutorialRoom>();

        private TutorialRoom _currentRoom;

        [Inject]
        private void Construct(DiContainer diContainer, LevelTransitionManager levelTransitionManager,
            LevelManager levelManager,
            ProgressSaver progressSaver, InputHandler inputHandler)
        {
            _progressSaver = progressSaver;
            _levelManager = levelManager;
            _diContainer = diContainer;
            _levelTransitionManager = levelTransitionManager;
            _inputHandler = inputHandler;
        }

        public void CreateTutorial()
        {
            foreach (var room in _tutorialRoomPrefabs)
            {
                var newRoom = Instantiate(room, _tutorialRoomParent);
                newRoom.gameObject.SetActive(false);
                newRoom.Room.CompletedRoom += Room_CompetedRoom;
                _tutorialRooms.Add(newRoom);
            }
        }

        private void Room_CompetedRoom(Room room)
        {
            StartCoroutine(EndLevelCoroutine(room));
            room.CompletedRoom -= Room_CompetedRoom;
        }

        public void StartTutorial()
        {
            _progressSaver.StartTutorial();
            EnterRoom(_tutorialRooms[0]);
        }

        public void EnterRoom(TutorialRoom tutorialRoom)
        {
            Debug.Log($"Trying to enter room {tutorialRoom}");
            if (!tutorialRoom) return;
            if (_inputHandler) _inputHandler.SetInputEnabled(InputLayer.LevelChange, false);
            _currentRoom = tutorialRoom;
            _projection.EnterRoom(_currentRoom.Room);
            _levelTransitionManager.PrepareRoom(_currentRoom.Room);
            _currentRoom.Room.SetActive();
            _backgroundPropSpawner.SetLevel(_currentRoom.Room.SpriteShapeController);
            _playerMovement.SetPosition(_currentRoom.Room.SpawnPointTransform.position);

            StartCoroutine(StartLevelCoroutineRoutine());
        }


        private int _currentRoomIndex;

        public void RestartTutorialStep()
        {
            Debug.Log($"Trying to restart tutorial step {_currentRoomIndex}");
            _currentRoom.Room.ExitRoom();
            var newRoom = Instantiate(_tutorialRoomPrefabs[_currentRoomIndex], _tutorialRoomParent);
            newRoom.gameObject.SetActive(false);
            newRoom.Room.CompletedRoom += Room_CompetedRoom;
            _tutorialRooms[_currentRoomIndex] = newRoom;
            EnterRoom(newRoom);
        }

        private IEnumerator EndLevelCoroutine(Room room)
        {
            var exitPortal = _levelTransitionManager.SpawnExitPortal(room);

            bool portalClosed = false;
            exitPortal.OnPortalTrigger += () =>
            {
                _levelTransitionManager.LevelTransitionStarted?.Invoke();
            };
            exitPortal.OnPortalClosed += () => portalClosed = true;
            while (!portalClosed)
                yield return null;

            room.ExitRoom();
            _currentRoomIndex++;
            if (_currentRoomIndex == _tutorialRooms.Count)
            {
                FinishTutorial();
            }
            else
            {
                var nextRoom = _tutorialRooms[_currentRoomIndex];

                EnterRoom(nextRoom);
            }

            yield return null;
        }

        private void FinishTutorial()
        {
            _progressSaver.CompleteTutorial();
            _levelManager.EnterFloor();
        }

        private IEnumerator StartLevelCoroutineRoutine()
        {
            TimeManager.Instance.SlowGame(0f);
            if (!_currentRoom.Room.ExitPortalSpawnPoint)
            {
                _currentRoom.Room.ExitPortalSpawnPoint = _currentRoom.Room.SpawnPointTransform;
            }

            yield return _levelTransitionManager.SpawnEnterPortal(_currentRoom.Room);

            _levelTransitionManager.LevelTransitionFinished?.Invoke();
            TimeManager.Instance.ResumeGame();
            _currentRoom.ShowLevelStart();
            yield return null;
        }
    }
}
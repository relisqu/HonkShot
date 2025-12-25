using System;
using Scripts.Other;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine;
using System.Collections;
using Scripts.Items;
using Scripts.LevelSystem.LevelGeneration.Factories;
using Scripts.LevelSystem.TechnicalScripts;
using Scripts.UI;
using UnityEngine.Serialization;
using Zenject;
using Random = UnityEngine.Random;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        private PlayerMovement _playerMovement;
        private LevelTransitionManager _levelTransitionManager;
        private LevelObjectsFactory _levelObjectsFactory;
        private LevelGenerator _levelGenerator;

        [SerializeField] private Projection _projection;


        private int _currentRoomIndex = 0;
        private int _levelsCompleted = 0;
        private Floor _currentFloor;

        public Action EnteredRoom;
        public Action CompletedRoom;

        public Room CurrentRoom => _currentFloor?.VisitedRooms[^1];


        [Inject]
        private void Construct(PlayerMovement playerMovement, LevelObjectsFactory levelObjectsFactory,
            LevelGenerator levelGenerator, LevelTransitionManager levelTransitionManager)
        {
            _playerMovement = playerMovement;
            _levelGenerator = levelGenerator;
            _levelObjectsFactory = levelObjectsFactory;
            _levelTransitionManager = levelTransitionManager;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void EnterFloor()
        {
            if (DebugMode.Instance.GeneratingLevels)
            {
                _currentFloor = _levelGenerator.GenerateFloor();
                EnterRoom(_currentFloor.Rooms[0]);
            }
            else
            {
                var rooms = FindObjectsOfType<Room>();
                _currentFloor = _levelGenerator.GenerateFloor(rooms);
                EnterRoom(_currentFloor.Rooms[0]);
            }
        }

        public void Room_CompletedRoom(Room room)
        {
            CompletedRoom?.Invoke();
            room.CompletedRoom -= Room_CompletedRoom;
        }

        public void EnterRoom(Room room)
        {
            Debug.Log($"Trying to enter room {room}");
            if (!room) return;
            room.CompletedRoom += Room_CompletedRoom;
            _projection.EnterRoom(room);
            _levelTransitionManager.PrepareRoom(room);
            _currentFloor.EnterRoom(room);
            CurrentRoom.SetActive();

            EnteredRoom?.Invoke();
            _playerMovement.SetPosition(CurrentRoom.SpawnPointTransform.position);

            StartCoroutine(StartLevelCoroutineRoutine());
        }

        private IEnumerator StartLevelCoroutineRoutine()
        {
            TimeManager.Instance.SlowGame(0f);
            if (!CurrentRoom.ExitPortalSpawnPoint)
            {
                CurrentRoom.ExitPortalSpawnPoint = CurrentRoom.SpawnPointTransform;
            }

            yield return _levelTransitionManager.SpawnEnterPortal(CurrentRoom);

            TimeManager.Instance.ResumeGame();
            yield return null;
        }


        private IEnumerator EndLevelCoroutine()
        {
            _levelsCompleted++;

            var exitPortal = _levelTransitionManager.SpawnExitPortal(CurrentRoom);

            bool portalClosed = false;
            exitPortal.OnPortalTrigger += () =>
            {
                //_projection.DestroySimulation();
            };
            exitPortal.OnPortalClosed += () => portalClosed = true;
            while (!portalClosed)
                yield return null;

            // Prepare for next room
            _currentRoomIndex++;
            var nextRoom = _currentFloor.GetRoom(_currentRoomIndex);

            // Destroy previous room if needed
            CurrentRoom.ExitRoom();


            yield return _levelTransitionManager.ShowEndLevelTransition(_levelsCompleted);


            if (nextRoom)
            {
                EnterRoom(nextRoom);
            }
            else
            {
                FinishFloor();
            }

            yield return null;
        }

        private void FinishFloor()
        {
            Debug.LogError("Finishing floor");
        }

        private void OnCompletedRoom()
        {
            StartCoroutine(EndLevelCoroutine());
        }

        private void OnEnable()
        {
            CompletedRoom += OnCompletedRoom;
        }

        private void OnDisable()
        {
            CompletedRoom -= OnCompletedRoom;
        }
    }
}
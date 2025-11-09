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
        private LevelObjectsFactory _levelObjectsFactory;
        private LevelGenerator _levelGenerator;

        [SerializeField] private BackgroundPropSpawner _backgroundPropSpawner;
        [SerializeField] private float _timeTransitionDuration = 1.2f;

        [SerializeField] private Projection _projection;

        [SerializeField] private ItemSelectionUI _itemSelectionUI;

        private int _currentRoomIndex = 0;
        private int _levelsCompleted = 0;
        private Floor _currentFloor;

        public Action EnteredRoom;
        public Action CompletedRoom;

        public Room CurrentRoom => _currentFloor?.VisitedRooms[^1];

        [Space] [Header(("Item generation"))] [Range(0f, 1f)] [SerializeField]
        private float _itemScreenSpawnChance;

        [Min(1)]
        [Tooltip("Once per how many rooms item room will try spawn - if select 1, item will try to spawn every room")]
        [SerializeField]
        private int _itemScreenPerRoomRate = 2;

        [Inject]
        private void Construct(PlayerMovement playerMovement, LevelObjectsFactory levelObjectsFactory,
            LevelGenerator levelGenerator)
        {
            _playerMovement = playerMovement;
            _levelGenerator = levelGenerator;
            _levelObjectsFactory = levelObjectsFactory;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void EnterFloor()
        {
            _currentFloor = _levelGenerator.GenerateFloor();
            EnterRoom(_currentFloor.Rooms[0]);
        }

        public void EnterRoom(Room room)
        {
            Debug.Log($"Trying to enter room {room}");
            if (!room) return;
            _backgroundPropSpawner.SetLevel(room.SpriteShapeController);
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

            var portal = _levelObjectsFactory.SpawnLevelEnterPortal(null);
            portal.transform.position = CurrentRoom.SpawnPointTransform.position;
            portal.PlayJumpOut(_playerMovement.GetTransform());

            yield return new WaitUntil(portal.FinishedAnimation);
            float t = 0f;
            while (t < _timeTransitionDuration)
            {
                TimeManager.Instance.SlowGame(Mathf.Lerp(0f, 1f, t / _timeTransitionDuration));
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            TimeManager.Instance.ResumeGame();
            yield return null;
        }

        private bool ShouldGiveItems()
        {
            var shouldTryGenerateRoom = _levelsCompleted % _itemScreenPerRoomRate == 0;
            if (shouldTryGenerateRoom)
            {
                return Random.value <= _itemScreenSpawnChance;
            }

            return false;
        }

        public void ShowItemSelectionUI()
        {
            if (ShouldGiveItems())
            {
                bool itemSelected = false;
                int selectedItem = -1;
                _itemSelectionUI.ShowItems(null);
            }
        }

        private IEnumerator EndLevelCoroutine()
        {
            _levelsCompleted++;

            var portal = _levelObjectsFactory.SpawnLevelExitPortal(null);
            portal.transform.position = CurrentRoom.ExitPortalSpawnPoint.position;

            bool portalClosed = false;
            portal.OnPortalTrigger += () => { _projection.DestroySimulation(); };
            portal.OnPortalClosed += () => portalClosed = true;
            while (!portalClosed)
                yield return null;

            // Prepare for next room
            _currentRoomIndex++;
            var nextRoom = _currentFloor.GetRoom(_currentRoomIndex);

            // Destroy previous room if needed
            CurrentRoom.ExitRoom();

            ShowItemSelectionUI();

            while (_itemSelectionUI.IsActive)
            {
                yield return null;
            }

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
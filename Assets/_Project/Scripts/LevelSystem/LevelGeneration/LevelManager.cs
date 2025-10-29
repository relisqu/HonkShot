using System;
using Scripts.Other;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine;
using System.Collections;
using Scripts.Items;
using Scripts.LevelSystem.TechnicalScripts;
using Scripts.UI;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        private Room _currentRoom;
        private Room _previousRoom;
        public Action EnteredRoom;
        public Action CompletedRoom;

        public Room CurrentRoom => _currentRoom;

        [SerializeField] private BackgroundPropSpawner _backgroundPropSpawner;
        [SerializeField] private Portal _portalPrefab;
        [SerializeField] private float _timeTransitionDuration = 1.2f;
        private bool _waitingForPlayer;

        [SerializeField] private LevelGenerator _levelGenerator;
        private int _currentRoomIndex = 0;

        [SerializeField] private Scripts.Player.Projection _projection;

        [SerializeField] private ItemSelectionUI _itemSelectionUI;
        private int _levelsCompleted = 0;

        private void Awake()
        {
            Instance = this;
            if (!DebugMode.Instance.GeneratingLevels)
            {
                var room = FindObjectOfType<Room>();
                _currentRoom = room;
            }
        }

        public void EnterRoom(Room room)
        {
            Debug.Log($"Trying to enter room {room}");
            if (!room) return;
            _backgroundPropSpawner.SetLevel(room.SpriteShapeController);
            if (_currentRoom)
                Destroy(_currentRoom.gameObject);
            _currentRoom = room;
            _currentRoom.SetActive();
            EnteredRoom?.Invoke();
            PointReceiver.Instance.transform.root.position = _currentRoom.SpawnPointTransform.position;
            StartCoroutine(LevelStartRoutine());
        }

        private IEnumerator LevelStartRoutine()
        {
            TimeManager.Instance.PauseGame();
            if (!_currentRoom.ExitPortalSpawnPoint)
            {
                _currentRoom.ExitPortalSpawnPoint = _currentRoom.SpawnPointTransform;
            }

            var portalScript = Instantiate(_portalPrefab, _currentRoom.SpawnPointTransform.position,
                Quaternion.identity);


            var playerTransform = PointReceiver.Instance.transform;
            if (portalScript)
            {
                playerTransform.position = _currentRoom.SpawnPointTransform.position;
                portalScript.PlayJumpOut(playerTransform);
            }

            yield return new WaitForSecondsRealtime(1f); // Replace with actual animation length

            float t = 0f;
            while (t < _timeTransitionDuration)
            {
                TimeManager.Instance.SlowGame(Mathf.Lerp(0f, 1f, t / _timeTransitionDuration));
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            TimeManager.Instance.ResumeGame();
        }

        private bool ShouldGiveItems()
        {
            // Every 3 levels, show item selection before portal
            return _levelsCompleted % 1 == 0;
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

        private IEnumerator LevelEndRoutine()
        {
            _levelsCompleted++;
            var portal = Instantiate(_portalPrefab, _currentRoom.ExitPortalSpawnPoint.position, Quaternion.identity);

            bool portalClosed = false;
            portal.OnPortalTrigger += () => { _projection.DestroySimulation(); };
            portal.OnPortalClosed += () => portalClosed = true;
            while (!portalClosed)
                yield return null;

            // Prepare for next room
            _currentRoomIndex++;
            var nextRoom = _levelGenerator.GetRoom(_currentRoomIndex);

            // Destroy previous room if needed
            if (_previousRoom && _previousRoom != _currentRoom)
            {
                Destroy(_previousRoom.gameObject);
            }

            _previousRoom = _currentRoom;

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
                // All rooms complete, generate a new batch and continue
                _levelGenerator.GenerateLevel();
                _currentRoomIndex = 0;
                var newRoom = _levelGenerator.GetRoom(_currentRoomIndex);
                if (newRoom)
                    EnterRoom(newRoom);
            }

            yield return null;
        }

        public void OnPlayerEnteredPortal()
        {
            _waitingForPlayer = false;
        }

        private void OnEnable()
        {
            CompletedRoom += OnCompletedRoom;
        }

        private void OnDisable()
        {
            CompletedRoom -= OnCompletedRoom;
        }

        private void OnCompletedRoom()
        {
            StartCoroutine(LevelEndRoutine());
        }
    }
}
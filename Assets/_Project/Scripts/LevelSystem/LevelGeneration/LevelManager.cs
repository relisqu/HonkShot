using System;
using Scripts.Other;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine;
using System.Collections;

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

        [SerializeField] private Portal _portalPrefab;
        [SerializeField] private float _timeTransitionDuration = 1.2f;
        private Portal _activePortal;
        private bool _waitingForPlayer;

        [SerializeField] private LevelGenerator _levelGenerator;
        private int _currentRoomIndex = 0;

        [SerializeField] private Scripts.Player.Projection _projection;

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

            if (_activePortal) Destroy(_activePortal);
            _activePortal = Instantiate(_portalPrefab, _currentRoom.SpawnPointTransform.position, Quaternion.identity);


            var playerTransform = PointReceiver.Instance.transform;
            var portalScript = _activePortal;
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

        private IEnumerator LevelEndRoutine()
        {
            _activePortal = Instantiate(_portalPrefab, _currentRoom.ExitPortalSpawnPoint.position, Quaternion.identity);

            bool portalClosed = false;
            _activePortal.OnPortalTrigger += () => { _projection.DestroySimulation(); };
            _activePortal.OnPortalClosed += () => portalClosed = true;
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
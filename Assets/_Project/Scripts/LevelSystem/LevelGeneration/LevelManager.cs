using System;
using Scripts.Other;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        private Room _currentRoom;
        public Action EnteredRoom;
        public Action CompletedRoom;

        public Room CurrentRoom => _currentRoom;


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
            if (room == null) return;
            _currentRoom = room;
            _currentRoom.SetActive();
            EnteredRoom?.Invoke();
        }
    }
}
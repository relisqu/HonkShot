using System;
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
        }

        public  void EnterRoom(Room room)
        {
            _currentRoom = room;
            _currentRoom.SetActive();
            EnteredRoom?.Invoke();
        }
    }
}
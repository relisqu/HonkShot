using System.Collections.Generic;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class Floor
    {
        public List<Room> Rooms = new List<Room>();

        private List<Room> _visitedRooms = new List<Room>();

        public List<Room> VisitedRooms => _visitedRooms;

        public void EnterRoom(Room room)
        {
            _visitedRooms.Add(room);
        }

        public Room GetRoom(int currentRoomIndex)
        {
            if (currentRoomIndex >= Rooms.Count)
                return null;
            return Rooms[currentRoomIndex];
        }
    }
}
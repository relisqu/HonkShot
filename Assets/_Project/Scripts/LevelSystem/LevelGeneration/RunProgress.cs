using System;
using Scripts.Services.Seed;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class RunProgress
    {
        public int CurrentFloorIndex { get; private set; }
        public int FloorsCompleted { get; private set; }
        public int TotalRoomsCompleted { get; private set; }
        public int RunSeed { get; private set; }

        public event Action<int> FloorStarted;
        public event Action<int> FloorCompleted;
        public event Action BossDefeated;
        public event Action RunCompleted;

        public RunProgress()
        {
            RunSeed = SeedUtility.CreateRandomSeed();
        }

        public void StartRun(int? seed = null)
        {
            Reset();
            RunSeed = seed ?? SeedUtility.CreateRandomSeed();
        }

        public int GetFloorSeed() => SeedUtility.Combine(RunSeed, SeedStream.FloorLayout, CurrentFloorIndex);

        public void OnFloorStarted()
        {
            FloorStarted?.Invoke(CurrentFloorIndex);
        }

        public void OnFloorCompleted()
        {
            FloorsCompleted++;
            FloorCompleted?.Invoke(CurrentFloorIndex);
        }

        public void OnBossDefeated()
        {
            BossDefeated?.Invoke();
        }

        public void OnRoomCompleted()
        {
            TotalRoomsCompleted++;
        }

        public bool AdvanceFloor(int totalFloors)
        {
            OnFloorCompleted();
            CurrentFloorIndex++;
            if (CurrentFloorIndex >= totalFloors)
            {
                RunCompleted?.Invoke();
                return false;
            }
            return true;
        }

        public void Reset()
        {
            CurrentFloorIndex = 0;
            FloorsCompleted = 0;
            TotalRoomsCompleted = 0;
        }
    }
}

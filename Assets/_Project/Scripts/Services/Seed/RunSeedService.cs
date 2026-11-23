using System;
using UnityEngine;

namespace Scripts.Services.Seed
{
    public class RunSeedService
    {
        // Static: the installer lives in the scene, so instance state would not survive the reload that starts the run.
        private static int? _pendingSeed;

        public int CurrentSeed { get; private set; }
        public string CurrentSeedText => SeedUtility.Format(CurrentSeed);
        public bool HasPendingSeed => _pendingSeed.HasValue;

        public event Action<int> RunStarted;

        public RunSeedService()
        {
            CurrentSeed = SeedUtility.CreateRandomSeed();
        }

        public void SetSeedForNextRun(int seed) =>
            _pendingSeed = seed;

        public int BeginRun()
        {
            CurrentSeed = _pendingSeed ?? SeedUtility.CreateRandomSeed();
            _pendingSeed = null;

            Debug.Log($"[RunSeed] Run started with seed {CurrentSeedText}");
            RunStarted?.Invoke(CurrentSeed);
            return CurrentSeed;
        }

        public System.Random CreateRandom(SeedStream stream, params int[] values) =>
            SeedUtility.CreateRandom(CurrentSeed, stream, values);
    }
}

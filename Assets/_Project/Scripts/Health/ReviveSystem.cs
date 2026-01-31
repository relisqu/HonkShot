using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Health
{
    public interface IReviveProvider
    {
        int Priority { get; }
        bool CanRevive { get; }
        bool TryRevive(HealthController healthController);
    }

    public class ReviveSystem : MonoBehaviour
    {
        public static ReviveSystem Instance { get; private set; }

        private List<IReviveProvider> _reviveProviders = new();

        public event Action<IReviveProvider> OnReviveUsed;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Register(IReviveProvider provider)
        {
            Debug.Log($"[ReviveSystem] Registering {provider.GetType().Name}");
            if (!_reviveProviders.Contains(provider))
            {
                _reviveProviders.Add(provider);
                _reviveProviders.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }

        public void Unregister(IReviveProvider provider)
        {
            _reviveProviders.Remove(provider);
        }

        public bool TryRevive(HealthController healthController)
        {
            Debug.Log("[ReviveSystem] Trying to revive: ");
            foreach (var provider in _reviveProviders)
            {
                if (provider.CanRevive && provider.TryRevive(healthController))
                {
                    OnReviveUsed?.Invoke(provider);
                    Debug.Log($"[ReviveSystem] Revived by {provider.GetType().Name}");
                    return true;
                }
            }

            Debug.Log("[ReviveSystem] Revive failed. ");
            return false;
        }

        public bool HasAvailableRevive()
        {
            foreach (var provider in _reviveProviders)
            {
                if (provider.CanRevive)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetAvailableReviveCount()
        {
            int count = 0;
            foreach (var provider in _reviveProviders)
            {
                if (provider.CanRevive)
                {
                    count++;
                }
            }

            return count;
        }
    }
}

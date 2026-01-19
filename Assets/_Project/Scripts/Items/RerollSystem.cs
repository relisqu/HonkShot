using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Items
{
    public interface IRerollProvider
    {
        int RemainingRerolls { get; }
        bool TryUseReroll();
    }

    public class RerollSystem : MonoBehaviour
    {
        public static RerollSystem Instance { get; private set; }

        private List<IRerollProvider> _providers = new();

        public event Action OnRerollUsed;

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

        public void Register(IRerollProvider provider)
        {
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public void Unregister(IRerollProvider provider)
        {
            _providers.Remove(provider);
        }

        public int GetTotalRerolls()
        {
            int total = 0;
            foreach (var provider in _providers)
            {
                total += provider.RemainingRerolls;
            }
            return total;
        }

        public bool TryUseReroll()
        {
            foreach (var provider in _providers)
            {
                if (provider.RemainingRerolls > 0 && provider.TryUseReroll())
                {
                    OnRerollUsed?.Invoke();
                    Debug.Log($"[RerollSystem] Reroll used from {provider.GetType().Name}");
                    return true;
                }
            }

            return false;
        }

        public bool HasRerollsAvailable()
        {
            return GetTotalRerolls() > 0;
        }
    }
}

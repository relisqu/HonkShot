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

    public class ItemPickBonusSystem : MonoBehaviour
    {
        private List<IRerollProvider> _rerollProviders = new();
        private List<IExtraPickProvider> _extraPickProviders = new();
        private int _pendingExtraPicks;

        public int PendingExtraPicks => _pendingExtraPicks;

        public event Action OnRerollUsed;
        public event Action<int> OnExtraPicksCalculated;

        public void RegisterRerollProvider(IRerollProvider provider)
        {
            if (!_rerollProviders.Contains(provider))
            {
                _rerollProviders.Add(provider);
            }
        }

        public void UnregisterRerollProvider(IRerollProvider provider)
        {
            _rerollProviders.Remove(provider);
        }

        public void RegisterExtraPickProvider(IExtraPickProvider provider)
        {
            if (!_extraPickProviders.Contains(provider))
            {
                _extraPickProviders.Add(provider);
            }
        }

        public void UnregisterExtraPickProvider(IExtraPickProvider provider)
        {
            _extraPickProviders.Remove(provider);
        }

        public int GetTotalRerolls()
        {
            int total = 0;
            foreach (var provider in _rerollProviders)
            {
                total += provider.RemainingRerolls;
            }
            return total;
        }

        public bool TryUseReroll()
        {
            foreach (var provider in _rerollProviders)
            {
                if (provider.RemainingRerolls > 0 && provider.TryUseReroll())
                {
                    OnRerollUsed?.Invoke();
                    Debug.Log($"[ItemPickBonusSystem] Reroll used from {provider.GetType().Name}");
                    return true;
                }
            }
            return false;
        }

        public void CalculateExtraPicks()
        {
            _pendingExtraPicks = 0;

            foreach (var provider in _extraPickProviders)
            {
                _pendingExtraPicks += provider.GuaranteedExtraPicks;

                if (provider.ExtraPickChance > 0 && UnityEngine.Random.value < provider.ExtraPickChance)
                {
                    _pendingExtraPicks++;
                    Debug.Log($"[ItemPickBonusSystem] Chance-based extra pick from {provider.GetType().Name}");
                }
            }

            if (_pendingExtraPicks > 0)
            {
                Debug.Log($"[ItemPickBonusSystem] Total extra picks: {_pendingExtraPicks}");
            }

            OnExtraPicksCalculated?.Invoke(_pendingExtraPicks);
        }

        public bool TryUseExtraPick()
        {
            if (_pendingExtraPicks <= 0) return false;

            _pendingExtraPicks--;
            Debug.Log($"[ItemPickBonusSystem] Extra pick used. Remaining: {_pendingExtraPicks}");
            return true;
        }

        public void ResetExtraPicks()
        {
            _pendingExtraPicks = 0;
        }
    }
}

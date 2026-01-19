using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Items
{
    public interface IExtraPickProvider
    {
        int GuaranteedExtraPicks { get; }
        float ExtraPickChance { get; }
        void OnExtraPickUsed();
    }

    public class ExtraPickSystem : MonoBehaviour
    {
        public static ExtraPickSystem Instance { get; private set; }

        private List<IExtraPickProvider> _providers = new();
        private int _pendingExtraPicks;

        public event Action<int> OnExtraPicksCalculated;

        public int PendingExtraPicks => _pendingExtraPicks;

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

        public void Register(IExtraPickProvider provider)
        {
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public void Unregister(IExtraPickProvider provider)
        {
            _providers.Remove(provider);
        }

        public void CalculateExtraPicks()
        {
            _pendingExtraPicks = 0;

            foreach (var provider in _providers)
            {
                _pendingExtraPicks += provider.GuaranteedExtraPicks;

                if (provider.ExtraPickChance > 0 && UnityEngine.Random.value < provider.ExtraPickChance)
                {
                    _pendingExtraPicks++;
                    Debug.Log($"[ExtraPickSystem] Chance-based extra pick from {provider.GetType().Name}");
                }
            }

            if (_pendingExtraPicks > 0)
            {
                Debug.Log($"[ExtraPickSystem] Total extra picks: {_pendingExtraPicks}");
            }

            OnExtraPicksCalculated?.Invoke(_pendingExtraPicks);
        }

        public bool TryUseExtraPick()
        {
            if (_pendingExtraPicks <= 0) return false;

            _pendingExtraPicks--;

            foreach (var provider in _providers)
            {
                provider.OnExtraPickUsed();
            }

            Debug.Log($"[ExtraPickSystem] Extra pick used. Remaining: {_pendingExtraPicks}");
            return true;
        }

        public void ResetExtraPicks()
        {
            _pendingExtraPicks = 0;
        }
    }
}

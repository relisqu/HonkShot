using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Other;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class RegenOnExitRoomItem : Item
    {
        private HealthController _healthController;
        private bool _tookDamageThisLevel = false;
        [SerializeField] private int _healthRegenAmount;

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[RegenOnExitRoomItem] Inited RegenOnExitRoomItem");
        }


        void Start()
        {
            _healthController = GetComponentInParent<HealthController>();
            if (LevelManager.Instance)
            {
                LevelManager.Instance.CompletedRoom += OnLevelComplete;
            }
        }

        void OnDestroy()
        {
            if (LevelManager.Instance)
            {
                LevelManager.Instance.CompletedRoom -= OnLevelComplete;
            }
        }


        private void OnLevelComplete()
        {
            if (_healthController)
            {
                if (DebugMode.Instance.DebugEnabled)
                    Debug.Log($"[RegenOnExitRoomItem] Regenerated {_healthRegenAmount} health.");
                float healAmount = _healthController.GetMaxHealth() * _healthRegenAmount;
                _healthController.AddHealth(healAmount);
            }
        }
    }
}
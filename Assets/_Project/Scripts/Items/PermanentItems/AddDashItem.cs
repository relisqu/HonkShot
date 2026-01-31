using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.Dash;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class AddDashItem : Item
    {
        [SerializeField] private int _dashesCount;

        private PlayerDashController _playerDashController;

        void Start()
        {
            _playerDashController = GetComponentInParent<PlayerDashController>();
            _playerDashController.AddMaxDashes(_dashesCount);
        }

        void OnDestroy()
        {
        }


        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}
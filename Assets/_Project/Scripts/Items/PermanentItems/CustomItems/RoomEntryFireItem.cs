using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class RoomEntryFireItem : Item
    {
        [SerializeField] private float _fireAmount = 10f;

        private void Start()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.EnteredRoom += LevelManager_EnteredRoom;
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.EnteredRoom -= LevelManager_EnteredRoom;
        }

        private void LevelManager_EnteredRoom()
        {
            if (GooseFireSystem.Instance)
                GooseFireSystem.Instance.AddFire(_fireAmount);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[RoomEntryFireItem] InitItem");
        }
    }
}

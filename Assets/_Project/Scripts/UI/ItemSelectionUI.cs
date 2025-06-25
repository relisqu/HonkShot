using UnityEngine;
using UnityEngine.UI;
using System;
using Scripts.Items;
using System.Collections.Generic;
using Scripts.PointSystem;

namespace Scripts.UI
{
    public class ItemSelectionUI : MonoBehaviour
    {
        public Button[] itemButtons; // Assign in inspector
        public GameObject panel; // Assign in inspector

        private Action<int> _onItemSelected;
        public bool IsActive => _isActive;
        private bool _isActive;

        [SerializeField] private ItemManager _itemManager;
        [SerializeField] private PlayerInventory _playerInventory;

        public void ShowItems(Action<int> onItemSelected)
        {
            _isActive = true;
            panel.SetActive(true);
            var items = _itemManager.GetRandomItems(3);
            _onItemSelected = onItemSelected;
            if (items.Count < 3) Skip();
            for (int i = 0; i < itemButtons.Length; i++)
            {
                int index = i;
                itemButtons[i].onClick.RemoveAllListeners();
                itemButtons[i].onClick.AddListener(() => SelectItem(items[index]));
            }
        }

        private void Skip()
        {
            _isActive = false;
            panel.SetActive(false);
        }

        private void SelectItem(PlayerItemSO playerItem)
        {
            _isActive = false;
            panel.SetActive(false);
            _playerInventory.AddItem(playerItem);
        }
    }
}
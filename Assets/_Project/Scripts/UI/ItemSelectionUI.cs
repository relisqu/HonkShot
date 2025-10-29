using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Scripts.Items;
using System.Collections.Generic;
using Scripts.PointSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using Scripts.Items.PlayerItemManager;

namespace Scripts.UI
{
    public class ItemSelectionUI : MonoBehaviour
    {
        public ItemSelectionSlotUI[] slots; // Assign in inspector
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
            if (items.Count <= 0) Skip();
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < items.Count)
                {
                    slots[i].Setup(items[i], OnItemSelected);
                    slots[i].gameObject.SetActive(true);
                    slots[i].transform.localPosition = new Vector3(1f, 0f, 1f);
                    StartCoroutine(ShowCoroutine());
                }
                else
                {
                    slots[i].gameObject.SetActive(false);
                }
            }
        }

        public IEnumerator ShowCoroutine()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].gameObject.activeSelf)
                {
                    slots[i].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InElastic);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        private void Skip()
        {
            _isActive = false;
            panel.SetActive(false);
        }

        private void OnItemSelected(PlayerItemSO item)
        {
            _isActive = false;
            panel.SetActive(false);
            _playerInventory.AddItem(item);
        }
    }
}
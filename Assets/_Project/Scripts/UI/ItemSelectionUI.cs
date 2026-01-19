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
using Scripts.Services.Localization;
using TMPro;
using Zenject;

namespace Scripts.UI
{
    public class ItemSelectionUI : MonoBehaviour
    {
        private ILocalizationService _localizationService;

        public ItemSelectionSlotUI[] slots; // Assign in inspector
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemDescriptionText;
        public GameObject panel; // Assign in inspector


        [SerializeField] private ItemManager _itemManager;
        [SerializeField] private PlayerInventory _playerInventory;

        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        private Action<int> _onItemSelected;
        public bool IsActive => _isActive;
        private bool _isActive;

        public void ShowItems(Action<int> onItemSelected)
        {
            _isActive = true;
            ClearTexts();
            panel.SetActive(true);
            var items = _itemManager.GetRandomItems(3);
            _onItemSelected = onItemSelected;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].gameObject)
                {
                    slots[i].StartedHover -= ItemSlot_StartedHover;
                    slots[i].FinishedHover -= ItemSlot_FinishedHover;
                }
            }

            if (items.Count <= 0) Skip();
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < items.Count)
                {
                    slots[i].Setup(items[i], OnItemSelected);
                    slots[i].StartedHover += ItemSlot_StartedHover;
                    slots[i].FinishedHover += ItemSlot_FinishedHover;
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

        private void ItemSlot_StartedHover(ItemSelectionSlotUI slotUI)
        {
            _itemDescriptionText.text = _localizationService.Get($"item_{slotUI.PlayerItemSO.Id}_description");
            _itemNameText.text = _localizationService.Get($"item_{slotUI.PlayerItemSO.Id}_title");
            
        }

        private void ClearTexts()
        {
            _itemDescriptionText.text = null;
            _itemNameText.text = null;
        }

        private void ItemSlot_FinishedHover(ItemSelectionSlotUI slotUI)
        {
            ClearTexts();
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
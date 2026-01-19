using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Scripts.Items;
using Scripts.Items.PlayerItemManager;
using Scripts.Services.Localization;
using TMPro;
using Zenject;

public class ItemSelectionSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ILocalizationService _localizationService;

    public Image icon;

    private PlayerItemSO _item;
    public Action<PlayerItemSO> SelectedItem;
    public Action<ItemSelectionSlotUI> StartedHover;
    public Action<ItemSelectionSlotUI> FinishedHover;
    
    public PlayerItemSO PlayerItemSO => _item;

    [Inject]
    private void Construct(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public void Setup(PlayerItemSO item, System.Action<PlayerItemSO> onSelected)
    {
        _item = item;
        SelectedItem = onSelected;
        if (icon && item.icon)
        {
            icon.sprite = item.icon;
        }
        else
        {
            icon.sprite = null;
        }


        transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectedItem?.Invoke(_item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartedHover?.Invoke(this);
        transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FinishedHover?.Invoke(this);
        transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
    }
}
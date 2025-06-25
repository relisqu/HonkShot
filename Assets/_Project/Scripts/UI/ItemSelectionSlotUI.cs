using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Scripts.Items;
using TMPro;

public class ItemSelectionSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private PlayerItemSO _item;
    private System.Action<PlayerItemSO> _onSelected;

    public void Setup(PlayerItemSO item, System.Action<PlayerItemSO> onSelected)
    {
        _item = item;
        _onSelected = onSelected;
        if (icon && item.icon) icon.sprite = item.icon;
        if (nameText) nameText.text = item.itemName;
        if (descriptionText) descriptionText.text = item.description;
        transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onSelected?.Invoke(_item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
    }
} 
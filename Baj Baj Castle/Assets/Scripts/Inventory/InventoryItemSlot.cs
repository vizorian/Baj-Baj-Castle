using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private GameObject stackContainer;

    [SerializeField]
    private TextMeshProUGUI stackCount;

    private InventoryItem item;

    public void Set(InventoryItem item)
    {
        this.item = item;

        icon.sprite = item.Data.Icon;
        label.text = item.Data.DisplayName;
        if (item.StackSize <= 1)
        {
            stackContainer.SetActive(false);
        }
        else
        {
            stackCount.text = item.StackSize.ToString();
        }
    }

    private string ToTooltipString()
    {
        string tooltip = "";

        if (item != null)
        {
            // name
            tooltip += item.Data.DisplayName + "\n";
            tooltip += new string('-', item.Data.DisplayName.Length) + "\n";
            // description
            tooltip += "Damage: " + item.Data.ItemProperties.Damage + " " + item.Data.ItemProperties.DamageType + " damage\n";
            tooltip += "Attack speed: " + item.Data.ItemProperties.Cooldown + " attacks per second\n";
            tooltip += "Critical chance: " + item.Data.ItemProperties.CriticalChance + "%\n";
            tooltip += "Reach: " + item.Data.ItemProperties.Range + " units\n";
            tooltip += "Speed: " + item.Data.ItemProperties.Speed + " units\n";
            tooltip += "Knockback: " + item.Data.ItemProperties.Knockback + "\n";
        }

        return tooltip;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.ShowTooltip_Static(ToTooltipString());
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Tooltip.HideTooltip_Static();
    }
}

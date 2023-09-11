using JetBrains.Annotations;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory;

public class InventoryItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField][UsedImplicitly] private Image icon;

    private InventoryItem item;

    [SerializeField][UsedImplicitly] private TextMeshProUGUI label;

    [SerializeField][UsedImplicitly] private GameObject stackContainer;

    [SerializeField][UsedImplicitly] private TextMeshProUGUI stackCount;

    // pointer enter event handler
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Show tooltip
        Tooltip.ShowTooltip_Static(ToTooltipString());
    }

    // Pointer exit event handler
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip
        Tooltip.HideTooltip_Static();
    }

    // Set item slot to new item
    public void Set(InventoryItem newItem)
    {
        this.item = newItem;

        icon.sprite = newItem.Data.Icon;
        label.text = newItem.Data.DisplayName;
        if (newItem.StackSize <= 1)
            stackContainer.SetActive(false);
        else
            stackCount.text = newItem.StackSize.ToString();
    }

    // Get item data in string form for tooltip
    private string ToTooltipString()
    {
        var tooltip = "";

        if (item == null) return tooltip;

        // name
        tooltip += item.Data.DisplayName + "\n";
        tooltip += new string('-', item.Data.DisplayName.Length) + "\n";
        // description
        tooltip += "Damage: " + item.Data.ItemProperties.Damage + " " + item.Data.ItemProperties.DamageType +
                   " damage\n";
        tooltip += "Attack speed: " + item.Data.ItemProperties.Cooldown + " attacks per second\n";
        tooltip += "Critical chance: " + item.Data.ItemProperties.CriticalChance + "%\n";
        tooltip += "Reach: " + item.Data.ItemProperties.Range + " units\n";
        tooltip += "Speed: " + item.Data.ItemProperties.Speed + " units\n";
        tooltip += "Knockback: " + item.Data.ItemProperties.Knockback + "\n";

        return tooltip;
    }
}
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] [UsedImplicitly] private Image icon;

    private InventoryItem item;

    [SerializeField] [UsedImplicitly] private TextMeshProUGUI label;

    [SerializeField] [UsedImplicitly] private GameObject stackContainer;

    [SerializeField] [UsedImplicitly] private TextMeshProUGUI stackCount;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.ShowTooltip_Static(ToTooltipString());
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Tooltip.HideTooltip_Static();
    }

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
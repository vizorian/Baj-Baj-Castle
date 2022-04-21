using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : Interactable
{
    public InventoryItemData ItemData;

    private protected override void OnCollide(Collider2D collider)
    {
        collisions = true;

        if (collider.tag == "Player")
        {
            Tooltip.ShowTooltip_Static(ToTooltipString(), transform.position);
            DrawHighlightFull(collider.gameObject);
        }

        collisions = false;
    }
    private protected override void OnInteraction()
    {
        // Pick up behaviour
        InventorySystem.Instance.Add(ItemData);
        Destroy(gameObject);
    }

    private string ToTooltipString()
    {
        string tooltip = "";

        if (ItemData != null)
        {
            // name
            tooltip += ItemData.DisplayName + "\n";
            tooltip += new string('-', ItemData.DisplayName.Length) + "\n";
            // description
            tooltip += "Damage: " + ItemData.ItemProperties.Damage + " " + ItemData.ItemProperties.DamageType + " damage\n";
            tooltip += "Attack speed: " + ItemData.ItemProperties.Cooldown + " attacks per second\n";
            tooltip += "Reach: " + ItemData.ItemProperties.Range + " units\n";
            tooltip += "Speed: " + ItemData.ItemProperties.Speed + " units\n";
            tooltip += "Knockback: " + ItemData.ItemProperties.Knockback + "\n";
        }

        return tooltip;
    }
}

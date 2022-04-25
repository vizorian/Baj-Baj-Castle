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
            DrawHighlightFull(collider.gameObject);
        }

        collisions = false;
    }

    private protected override void OnInteraction()
    {
        if (InventorySystem.Instance.Add(ItemData))
        {
            Destroy(gameObject);
        }
        else
        {
            FloatingText.Create("Inventory is full", Color.red, transform.position, 1, 0.5f, 0.5f);
        }
    }
}

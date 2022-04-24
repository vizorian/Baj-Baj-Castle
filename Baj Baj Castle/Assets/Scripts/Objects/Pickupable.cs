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
        InventorySystem.Instance.Add(ItemData);
        Destroy(gameObject);
    }
}

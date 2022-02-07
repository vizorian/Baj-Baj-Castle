using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : Interactable
{
    public InventoryItemData ItemData;
    private protected bool isPickedup = false;

    private protected override void OnCollide(Collider2D collider)
    {
        if (collider.tag == "Player" && !isPickedup)
        {
            drawHighlights = true;
        }

        if (drawHighlights)
        {
            DrawHighlightFull(collider.gameObject);
            drawHighlights = false;
        }
        //if(collider.tag == "Player")
        //    OnCollect();
    }

    private protected override void OnInteraction()
    {
        Debug.Log("Picked up item");

        // pick up behaviour
        // add to collider inventory
        // delete self

        InventorySystem.Instance.Add(ItemData);
        Destroy(gameObject);
    }
}

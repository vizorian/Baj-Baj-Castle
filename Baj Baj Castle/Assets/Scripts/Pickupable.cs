using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : Interactable
{
    private protected bool isPickedup = false;

    protected override void OnCollide(Collider2D collider)
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

    protected override void OnInteraction()
    {
        Debug.Log("Picked up item");
        isPickedup = true;

        // pick up behaviour
        // add to collider inventory
        // delete self
    }
}

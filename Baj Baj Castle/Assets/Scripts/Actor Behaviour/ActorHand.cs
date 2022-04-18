using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorHand : MonoBehaviour
{
    public bool isFreezing = false;

    private bool newSelection = false;
    private bool isTurned = false;

    private float handSpeed = 1f;
    private float handRange;

    private Vector3 bodyPosition;
    private InventoryItem heldItem;
    private Vector3 heldItemHandlePosition;

    private GameObject itemObject;

    // Hand initialization from Actor
    public void Init(float handRange)
    {
        this.handRange = handRange;
    }

    private void Update()
    {
        UpdateHeldItem();
        UpdateVerticalRendering();
        if (!isFreezing)
            UpdateHorizontalRendering();
        //PrintSpeeds();
    }

    // Flips the hand and held item based on horizontal position
    private void UpdateHorizontalRendering()
    {
        if (Vector2.Dot(transform.parent.transform.right, transform.position - bodyPosition) > 0) // If to the right of actor
            transform.localScale = new Vector2(1, 1);
        else // If to the left of actor
            transform.localScale = new Vector2(-1, 1);
    }

    // Updates rendering for the hand and held item based on vertical position
    private void UpdateVerticalRendering()
    {
        SpriteRenderer handRenderer = transform.GetComponent<SpriteRenderer>();
        SpriteRenderer itemRenderer = null;

        if (heldItem != null)
            itemRenderer = itemObject.GetComponent<SpriteRenderer>();

        if (Vector2.Dot(transform.parent.transform.up, transform.position - bodyPosition) > 0) // If above actor
        {
            handRenderer.sortingLayerName = "Actor";
            if (itemRenderer != null)
                itemRenderer.sortingLayerName = "Actor";
        }
        else // If below actor
        {
            handRenderer.sortingLayerName = "Hand";
            if (itemRenderer != null)
                itemRenderer.sortingLayerName = "Hand";
        }
    }

    // Creates a new instance of the held item if there needs to be one
    private void UpdateHeldItem()
    {
        if (newSelection || heldItem == null)
        {
            Destroy(itemObject);

            if (heldItem != null && newSelection)
                InstantiateHeldItem();
        }
    }

    // Turns currently held item 90 degrees
    public void TurnHeldItem()
    {
        if (heldItem == null)
            return;

        if (!isTurned)
        {
            itemObject.transform.Rotate(0, 0, 90);
            isTurned = true;
        }
        else
        {
            itemObject.transform.Rotate(0, 0, -90);
            isTurned = false;
        }

        RealignHeldItem();
    }

    private void InstantiateHeldItem()
    {
        // Prefab preparation for actor use
        itemObject = Instantiate(heldItem.Data.Prefab, transform);
        itemObject.tag = "Untagged";
        Destroy(itemObject.GetComponent<Pickupable>());
        Destroy(itemObject.GetComponent<Collider2D>());

        itemObject.GetComponent<SpriteRenderer>().sortingLayerName = "Hand";
        itemObject.GetComponent<SpriteRenderer>().sortingOrder = 0;

        var item = itemObject.AddComponent<Item>();
        ItemProperties props = heldItem.Data.ItemProperties;
        item.Type = heldItem.Data.itemType;
        item.Damage = props.Damage;
        item.Speed = props.Speed;

        AlignHeldItem();

        newSelection = false;
    }

    // Moves the item so that the handle point is on the hand
    private void AlignHeldItem()
    {
        // Works for no flip
        float offsetX = Mathf.Abs(heldItemHandlePosition.x);
        float offsetY = Mathf.Abs(heldItemHandlePosition.y);

        var localDestination = new Vector3(offsetX, offsetY);
        itemObject.transform.localPosition = localDestination;
    }

    // Realigns the item after a flip so that the handle point is on the hand
    private void RealignHeldItem()
    {
        Vector2 newPos = new Vector2(-itemObject.transform.localPosition.y, -itemObject.transform.localPosition.x);
        itemObject.transform.localPosition = newPos;
    }

    public void SetHeldItem(InventoryItem item)
    {
        heldItem = item;

        if (item.Data.Prefab.transform.childCount != 0)
        {
            Vector3 childPos = item.Data.Prefab.transform.GetChild(0).gameObject.transform.localPosition;
            heldItemHandlePosition = new Vector3(childPos.x, childPos.y);
        }
        else
            heldItemHandlePosition = new Vector3(0, 0);

        newSelection = true;
        if (isTurned)
            TurnHeldItem();
    }

    public void ClearHeldItem()
    {
        heldItem = null;
    }

    public void UpdateCenterPosition(Vector2 position)
    {
        bodyPosition = position;
    }

    public void LookTowards(Vector3 lookTarget)
    {
        // Moves hand
        MoveTowards(lookTarget);

        //Rotates hand
        if (!isFreezing)
            RotateTowards(lookTarget);

        // FAILURE: Interesting effect for menu
        //transform.position = Vector2.MoveTowards(transform.position, lookTarget, _range);

        // Calculating the angle of the target relative to the actor
        //float z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
        //if (z < 0) z = 180 + (180 - Mathf.Abs(z));
    }

    // Move Hand towards target within range
    private void MoveTowards(Vector3 target)
    {
        Vector2 targetPos = target;
        var oldPos = transform.position;

        // If mouse is out of range
        if (Vector2.Distance(bodyPosition, target) > handRange)
        {
            // Move towards target
            transform.position = Vector2.MoveTowards(transform.position, targetPos, handSpeed * Time.deltaTime);

            // Post step position is out of range
            if (Vector2.Distance(transform.position, bodyPosition) > handRange)
            {
                // -------------------------------------------
                // Reset & Moving to border
                // -------------------------------------------

                // Reset position before step
                transform.position = oldPos;

                // Getting step size
                float step = handSpeed * Time.deltaTime;

                // Reduce step size by remaining distance to border
                float distanceToBorder = handRange - Vector2.Distance(bodyPosition, oldPos);
                step -= distanceToBorder;

                // Move position the remaining distance to the border
                transform.position = Vector2.MoveTowards(transform.position, targetPos, distanceToBorder);

                // -------------------------------------------
                // Movement via range boundary
                // -------------------------------------------

                // Body to current position
                Vector2 bodyToCurrent = bodyPosition - transform.position;
                // Body to target position
                Vector2 bodyToTarget = bodyPosition - target;

                // Calculating rotation angle based on remaining step size
                // Use this to rotate from current position !!!
                var angleNextPoint = (step / handRange) * Mathf.Rad2Deg;

                // Rotation of currentPos
                Quaternion currentRot = Quaternion.LookRotation(Vector3.forward, bodyToCurrent);

                // Rotation of targetPos
                Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, bodyToTarget);

                // Rotate by nextPoint angle
                float angle = Quaternion.Angle(currentRot, targetRot);

                // Getting current Z rotation values
                float currentZ = currentRot.eulerAngles.z;
                float targetZ = targetRot.eulerAngles.z;

                // Converting euler to range of -180;180
                if (currentZ > 180)
                    currentZ -= 360;
                if (targetZ > 180)
                    targetZ -= 360;

                // Calculate rotation direction
                if ((currentZ >= 0 && targetZ >= 0) || (currentZ < 0 && targetZ < 0)) // If both are positive or negative
                {
                    if (currentZ > targetZ) // If negative rotation
                    {
                        angle *= -1;
                        angleNextPoint *= -1;
                    }
                }
                else if ((currentZ >= 0 && currentZ < 90) || currentZ <= -90)
                {
                    angle *= -1;
                    angleNextPoint *= -1;
                }

                Vector3 bodyToNewTarget;
                // Rotate the current rotation vector
                if (Quaternion.Angle(currentRot, targetRot) > Math.Abs(angleNextPoint)) // If out of step range
                    bodyToNewTarget = Quaternion.Euler(0, 0, angleNextPoint) * bodyToCurrent;
                else // If inside step range
                    bodyToNewTarget = Quaternion.Euler(0, 0, angle) * bodyToCurrent;

                // Apply final movement
                Vector2 newTargetPos = bodyPosition - bodyToNewTarget;
                transform.position = Vector2.MoveTowards(transform.position, newTargetPos, Mathf.Infinity);
            }


        }
        else // Move to mouse within range
            transform.position = Vector2.MoveTowards(transform.position, targetPos, handSpeed * Time.deltaTime);
    }

    private void RotateTowards(Vector3 target)
    {
        Vector2 direction = target - bodyPosition;
        transform.up = direction;
    }
}

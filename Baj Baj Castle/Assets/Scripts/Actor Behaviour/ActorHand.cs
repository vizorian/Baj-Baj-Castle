using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorHand : MonoBehaviour
{
    public bool isHolding = false;
    
    private bool newSelection = false;
    private bool isTurned = false;
    
    private float _handSpeed = 1f;
    private float _range;
    
    private Vector3 _bodyPosition;
    private InventoryItem _heldItem;
    private Vector3 _heldItemHandlePosition;

    private GameObject _item;

    private float targetAcceleration = 40f;
    private float velocity = 0f;
    private float acceleration = 0f;



    // Hand initialization from Actor
    public void Init(float range)
    {
        _range = range;
    }

    private void Update()
    {
        UpdateHeldItem();
        //PrintSpeeds();
    }

    private void PrintSpeeds()
    {
        //print($"Current velocity is: {velocity}");
        //print($"Current Acceleration is: {acceleration}");
        if(acceleration >= targetAcceleration)
            print($"FAST ENOUGH!!!  Current Acceleration is: {acceleration}");
    }

    private void RecalculateSpeeds(Vector3 currentPos, Vector3 oldPos)
    {
        var oldVelocity = velocity;
        velocity = Vector2.Distance(currentPos, oldPos) / Time.deltaTime;

        acceleration = (Mathf.Abs(velocity - oldVelocity)) / Time.deltaTime;
    }

    // Creates a new instance of the held item if there needs to be one
    private void UpdateHeldItem()
    {
        if (newSelection || _heldItem == null)
        {
            Destroy(_item);

            if (_heldItem != null && newSelection)
                InstantiateHeldItem();
        }
    }

    // Turns currently held item 90 degrees
    public void TurnHeldItem()
    {
        if (_heldItem == null)
            return;

        if (!isTurned)
        {
            _item.transform.Rotate(0, 0, 90);
            isTurned = true;
        }
        else
        {
            _item.transform.Rotate(0, 0, -90);
            isTurned = false;
        }

        RealignHeldItem();
    }

    private void InstantiateHeldItem()
    {
        // Removes unneeded components from prefab
        _item = Instantiate(_heldItem.Data.Prefab, transform);
        _item.tag = "Untagged";
        Destroy(_item.GetComponent<Pickupable>());
        Destroy(_item.GetComponent<Collider2D>());
        _item.GetComponent<SpriteRenderer>().sortingLayerName = "Hand";
        _item.GetComponent<SpriteRenderer>().sortingOrder = 0;

        AlignHeldItem();

        newSelection = false;
    }

    // Moves the item so that the handle point is on the hand
    private void AlignHeldItem()
    {
        // Works for no flip
        float offsetX = Mathf.Abs(_heldItemHandlePosition.x);
        float offsetY = Mathf.Abs(_heldItemHandlePosition.y);

        var localDestination = new Vector3(offsetX, offsetY);
        _item.transform.localPosition = localDestination;
    }

    // Realigns the item after a flip so that the handle point is on the hand
    private void RealignHeldItem()
    {
        Vector2 newPos = new Vector2(-_item.transform.localPosition.y, -_item.transform.localPosition.x);
        _item.transform.localPosition = newPos;
    }

    public void SetHeldItem(InventoryItem item)
    {
        _heldItem = item;

        if (item.Data.Prefab.transform.childCount != 0)
        {
            Vector3 childPos = item.Data.Prefab.transform.GetChild(0).gameObject.transform.localPosition;
            _heldItemHandlePosition = new Vector3(childPos.x, childPos.y);
        }
        else
            _heldItemHandlePosition = new Vector3(0, 0);

        newSelection = true;
        if(isTurned)
            TurnHeldItem();
    }

    public void ClearHeldItem()
    {
        _heldItem = null;
    }

    public void UpdateCenterPosition(Vector2 position)
    {
        _bodyPosition = position;
    }

    public void LookTowards(Vector3 lookTarget)
    {
        // Moves hand
        MoveTowards(lookTarget);

        //Rotates hand
        if(!isHolding)
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
        if (Vector2.Distance(_bodyPosition, target) > _range)
        {
            // Move towards target
            transform.position = Vector2.MoveTowards(transform.position, targetPos, _handSpeed * Time.deltaTime);

            // Post step position is out of range
            if (Vector2.Distance(transform.position, _bodyPosition) > _range)
            {
                // -------------------------------------------
                // Reset & Moving to border
                // -------------------------------------------

                // Reset position before step
                transform.position = oldPos;

                // Getting step size
                float step = _handSpeed * Time.deltaTime;

                // Reduce step size by remaining distance to border
                float distanceToBorder = _range - Vector2.Distance(_bodyPosition, oldPos);
                step -= distanceToBorder;

                // Move position the remaining distance to the border
                transform.position = Vector2.MoveTowards(transform.position, targetPos, distanceToBorder);

                // -------------------------------------------
                // Movement via range boundary
                // -------------------------------------------
                
                // Body to current position
                Vector2 bodyToCurrent = _bodyPosition - transform.position;
                // Body to target position
                Vector2 bodyToTarget = _bodyPosition - target;

                // Calculating rotation angle based on remaining step size
                // Use this to rotate from current position !!!
                var angleNextPoint = (step / _range) * Mathf.Rad2Deg;

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
                Vector2 newTargetPos = _bodyPosition - bodyToNewTarget;
                transform.position = Vector2.MoveTowards(transform.position, newTargetPos, Mathf.Infinity);
            }


        }
        else // Move to mouse within range
            transform.position = Vector2.MoveTowards(transform.position, targetPos, _handSpeed * Time.deltaTime);

        RecalculateSpeeds(transform.position, oldPos);
    }

    private void RotateTowards(Vector3 target)
    {
        Vector2 direction = target - _bodyPosition;
        transform.up = direction;
    }
}

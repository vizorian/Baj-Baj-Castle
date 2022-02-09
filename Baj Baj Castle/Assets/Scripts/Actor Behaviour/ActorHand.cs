using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorHand : MonoBehaviour
{
    private bool newSelection = false;

    private bool isTurned = false;

    private bool atRange = false;
    private float _handSpeed = 1f;
    private float _range;
    private Vector3 _bodyPosition;

    private InventoryItem _heldItem;
    private Vector3 _heldItemHandlePosition;

    private GameObject _item;

    public void Init(float range)
    {
        _range = range;
    }

    private void Update()
    {
        UpdateHeldItem();
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

    public void LookTowards(Vector3 lookTarget) // TODO: make this weapon type dependant (potions/swords are vertical, daggers/spears are forwards)
    {
        // Moves hand
        MoveTowards(lookTarget);

        //Rotates hand
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
        Vector2 direction = target;

        // If mouse is out of range
        if (Vector2.Distance(_bodyPosition, target) > _range)
        {
            // Put hand from it's current position
            // To mouse position at limited range
            //transform.position = _position;

            var oldPos = transform.position;
            transform.position = Vector2.MoveTowards(transform.position, direction, _handSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, _bodyPosition) > _range)
            {
                transform.position = oldPos;
                atRange = true;
            }

            // POTENTIAL TODO: make the snapped circle movement 
            if (atRange)
            {
                transform.position = _bodyPosition;
                transform.position = Vector2.MoveTowards(transform.position, direction, direction.normalized.magnitude * _range);
                atRange = false;
            }
        }
        else // Move to mouse within range
            transform.position = Vector2.MoveTowards(transform.position, direction, _handSpeed * Time.deltaTime);
    }

    private void RotateTowards(Vector3 target)
    {
        Vector2 direction = target - _bodyPosition;
        transform.up = direction;
    }
}

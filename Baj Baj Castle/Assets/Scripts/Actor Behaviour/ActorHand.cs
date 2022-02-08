using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorHand : MonoBehaviour
{
    private float _range;
    private Vector2 _position;

    private SpriteRenderer _spriteRenderer;
    private InventoryItem _heldItem;

    private Sprite _handSprite;

    public void Init(float range)
    {
        _range = range;
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _handSprite = _spriteRenderer.sprite;
    }

    private void Update()
    {
        if (_heldItem != null)
            _spriteRenderer.sprite = _heldItem.Data.Prefab.GetComponent<SpriteRenderer>().sprite;
        else
            _spriteRenderer.sprite = _handSprite;

    }

    public void SetHeldItem(InventoryItem item)
    {
        _heldItem = item;
    }

    public void ClearHeldItem()
    {
        _heldItem = null;
    }

    public void UpdateCenterPosition(Vector2 position)
    {
        _position = position;
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
        if (Vector2.Distance(_position, target) > _range)
        {
            // Put hand from it's current position
            // To mouse position at limited range
            transform.position = _position;
            transform.position = Vector2.MoveTowards(transform.position, direction, direction.normalized.magnitude * _range);
        }
        else // Move to mouse within range
            transform.position = Vector2.MoveTowards(transform.position, direction, Mathf.Infinity);
    }

    private void RotateTowards(Vector3 target)
    {
        Vector2 direction = target;
        transform.up = direction - _position;
    }
}

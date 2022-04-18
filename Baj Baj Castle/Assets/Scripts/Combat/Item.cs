using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Collidable
{
    private EdgeCollider2D _edgeCollider;

    public ItemType Type;
    public float Damage;
    public float Speed;
    public float Cooldown;
    public float Knockback;
    public float Range;

    private protected override void Start()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private protected override void FixedUpdate()
    {
        if (Type == ItemType.Weapon)
        {
            _edgeCollider.OverlapCollider(ContactFilter, _hits);
            for (int i = 0; i < _hits.Count; i++)
            {
                OnCollide(_hits[i]);
                _hits[i] = null;
            }
        }
    }

    private protected override void OnCollide(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player" || collider.gameObject.tag == "Object" || collider.gameObject.tag == "Actor")
            print($"Weapon collided with: {collider.name}");
    }
}

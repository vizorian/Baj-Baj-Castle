using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Collidable
{
    private EdgeCollider2D _edgeCollider;

    public ItemType itemType;
    public float Damage;
    public float Speed;

    // Start is called before the first frame update
    private protected override void Start()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
    }

    // Update is called once per frame
    private protected override void FixedUpdate()
    {
        if(itemType == ItemType.Weapon)
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
        print($"Weapon collided with: {collider.name}");
    }
}

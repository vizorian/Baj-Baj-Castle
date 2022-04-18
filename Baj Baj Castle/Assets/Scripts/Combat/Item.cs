using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Collidable
{
    private EdgeCollider2D _edgeCollider;

    public ItemType Type;
    public float Damage;
    public DamageType DamageType;
    public float Speed;
    public float Cooldown;
    public float CooldownTimer;
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
        // Do timer
        if (CooldownTimer > 0)
        {
            CooldownTimer -= Time.deltaTime;
            return;
        }
        // If item is weapon and collider is a player or object or actor
        if (Type == ItemType.Weapon)
        {
            if (collider.gameObject.tag == "Player" || collider.gameObject.tag == "Object" || collider.gameObject.tag == "Actor")
            {
                // get Actor this item is attached to
                Actor actor = GetComponentInParent<Actor>();

                // If collider is the owner of the item
                if (collider.gameObject == actor.gameObject) return;

                var damageData = new DamageData(Damage, DamageType, Knockback, actor);
                collider.gameObject.SendMessage("TakeDamage", damageData);
            }
        }
        CooldownTimer = Cooldown;
    }
}

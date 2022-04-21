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
    public float CriticalChance;
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
        // Attack cooldown
        if (CooldownTimer > 0)
        {
            CooldownTimer -= Time.deltaTime;
            return;
        }

        // If item is weapon
        if (Type == ItemType.Weapon)
        {
            if (collider.gameObject.tag == "Player"
                || collider.gameObject.tag == "Actor"
                || collider.gameObject.tag == "Object")
            {
                // get Actor this item is attached to
                Actor actor = GetComponentInParent<Actor>();

                // if collider is the owner of the item
                if (collider.gameObject == actor.gameObject) return;

                // check for critical hit
                var damage = Damage;
                var knockback = Knockback;
                bool isCritical = UnityEngine.Random.Range(0f, 100f) <= CriticalChance;
                if (isCritical)
                {
                    damage *= 2;
                    knockback *= 2;
                }

                var damageData = new DamageData(damage, DamageType, knockback, actor, isCritical);
                collider.gameObject.SendMessage("TakeDamage", damageData);
            }
        }
        CooldownTimer = Cooldown;
    }
}
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

    private protected override void Awake()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private protected override void FixedUpdate()
    {
        // Cooldown
        if (CooldownTimer > 0)
        {
            CooldownTimer -= Time.deltaTime;
        }

        if (Type == ItemType.Weapon)
        {
            _edgeCollider.OverlapCollider(contactFilter, _hits);
            for (int i = 0; i < _hits.Count; i++)
            {
                OnCollide(_hits[i]);
                _hits[i] = null;
            }
        }
    }

    // Use item
    public bool Use(Actor actor)
    {
        if (CooldownTimer <= 0)
        {
            if (Type == ItemType.Consumable)
            {
                var healingAmount = actor.MaxHealth / 100f * Damage;
                actor.Heal(healingAmount);
            }
            CooldownTimer = Cooldown;
            return true;
        }
        return false;
    }

    private protected override void OnCollide(Collider2D collider)
    {
        // If item is weapon
        if (Type == ItemType.Weapon)
        {
            // get Actor this item is attached to
            Actor actor = GetComponentInParent<Actor>();
            if (CooldownTimer <= 0 && actor.Hand.Velocity >= actor.Hand.HandSpeed * Time.deltaTime * 2 / 3)
            {
                if (collider.gameObject.tag == "Actor"
                    || collider.gameObject.tag == "Object"
                    && collider.gameObject.tag != "Player")
                {


                    // if collider is the owner of the item
                    if (collider.gameObject == actor.gameObject) return;

                    // check for critical hit
                    var damage = Damage + (int)(actor.Strength * 0.2f);
                    var knockback = Knockback + actor.Strength * 0.1f;
                    var damageData = new DamageData(damage, DamageType, knockback, actor);
                    damageData.IsCritical = UnityEngine.Random.Range(0, 101) <= CriticalChance + actor.Luck * 0.5f;
                    collider.gameObject.SendMessage("TakeDamage", damageData);
                }
                CooldownTimer = Cooldown;
            }
        }
    }
}
using Actor_Behaviour;
using Enums;
using Objects;
using UnityEngine;

namespace Combat;

public class Item : Collidable
{
    public ItemType Type;
    public float Damage;
    public DamageType DamageType;
    public float Cooldown;
    public float CriticalChance;
    public float Knockback;
    public float Range;
    public float Speed;

    public float CooldownTimer;
    private EdgeCollider2D edgeCollider;
    private bool isCharged;
    private const float UnchargeTime = 0.2f;
    private float unchargeTimer;

    private protected override void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private protected override void FixedUpdate()
    {
        var actor = GetComponentInParent<Actor>();
        var chargingCriteria = actor.Hand.HandSpeed * Time.deltaTime * 5 / 7;
        if (isCharged)
        {
            if (actor.Hand.Velocity < chargingCriteria) unchargeTimer += Time.deltaTime;
            if (unchargeTimer >= UnchargeTime)
            {
                isCharged = false;
                unchargeTimer = 0;
            }
        }
        else
        {
            if (actor.Hand.Velocity >= chargingCriteria) isCharged = true;
        }

        // Cooldown
        if (CooldownTimer > 0) CooldownTimer -= Time.deltaTime;

        if (Type == ItemType.Weapon)
        {
            edgeCollider.OverlapCollider(ContactFilter, Hits);
            for (var i = 0; i < Hits.Count; i++)
            {
                OnCollide(Hits[i]);
                Hits[i] = null;
            }
        }
    }

    // Use item on actor
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

    // Handle collisions
    private protected override void OnCollide(Collider2D otherCollider)
    {
        // If item is weapon
        if (Type == ItemType.Weapon)
        {
            // get Actor this item is attached to
            var actor = GetComponentInParent<Actor>();
            if (CooldownTimer <= 0 && isCharged)
                if (otherCollider.gameObject.tag == "Actor"
                    || (otherCollider.gameObject.tag == "Object"
                        && otherCollider.gameObject.tag != "Player"))
                {
                    // if otherCollider is the owner of the item
                    if (otherCollider.gameObject == actor.gameObject) return;

                    // check for critical hit
                    var damage = Damage + (int)(actor.Strength * 0.2f);
                    var knockback = Knockback + actor.Strength * 0.1f;
                    var damageData = new DamageData(damage, DamageType, knockback, actor)
                    {
                        IsCritical = Random.Range(0, 101) <= CriticalChance + actor.Luck * 0.5f
                    };
                    otherCollider.gameObject.SendMessage("TakeDamage", damageData);
                    CooldownTimer = Cooldown;
                    isCharged = false;
                    GetComponent<SpriteRenderer>().color = Color.white;
                }
        }
    }
}
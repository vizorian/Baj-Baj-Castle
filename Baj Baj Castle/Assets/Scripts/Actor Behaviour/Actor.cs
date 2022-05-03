using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private protected List<Collider2D> Hits = new List<Collider2D>();

    public ActorType ActorType;
    public int Agility;
    public Sprite BackSprite;
    private protected BoxCollider2D BoxCollider;
    private protected ContactFilter2D ContactFilter;
    public int Defense;
    public Sprite FrontSprite;
    public ActorHand Hand;
    public GameObject HandPrefab;

    // Attributes
    public float Health;
    public int Intelligence;

    private protected GameObject InteractionObject;

    // Range attributes
    public float InteractionRange;
    public bool IsActive = false;
    private protected Vector3 KnockbackDirection;
    public int Luck;
    public float MaxHealth;
    private protected Vector3 MoveDelta;
    public float MovementSpeed;
    public float ReachRange;
    public int Resistance;

    private protected Rigidbody2D RigidBody;
    public Sprite SideSprite;
    private protected SpriteRenderer SpriteRenderer;
    public int Strength;
    private protected GameObject Target;

    private protected virtual void Awake()
    {
        // Atribute usage 
        MovementSpeed += Agility * 0.01f;
        MaxHealth += Strength;
        Resistance += (int)(Strength + Intelligence * 0.5f);
        Defense += (int)(Intelligence * 0.2f + Agility * 0.01f);
        Health = MaxHealth;


        RigidBody = GetComponent<Rigidbody2D>();
        BoxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        ContactFilter = new ContactFilter2D();

        if (HandPrefab == null) return;
        Hand = Instantiate(HandPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform)
            .GetComponent<ActorHand>();
        Hand.Init(ReachRange);
        Hand.UpdateCenterPosition(transform.position);
    }

    private protected virtual void FixedUpdate()
    {
        // Handling collisions
        BoxCollider.OverlapCollider(ContactFilter, Hits);
        for (var i = 0; i < Hits.Count; i++)
        {
            OnCollide(Hits[i]);
            Hits[i] = null;
        }
    }

    private protected virtual void OnCollide(Collider2D otherCollider)
    {
    }

    // Take damage, called by weapons on collision
    [UsedImplicitly]
    private protected virtual void TakeDamage(DamageData damageData)
    {
        var knockback = damageData.Knockback;
        var damage = damageData.Amount;

        // Adjust damage based on if the weapon is flipped or not
        if (damageData.Source.Hand != null)
        {
            if (damageData.Type == DamageType.Piercing && damageData.Source.Hand.IsItemTurned)
                damageData.Type = DamageType.Slashing;
            else if (damageData.Type == DamageType.Slashing && damageData.Source.Hand.IsItemTurned)
                damageData.Type = DamageType.Piercing;
        }

        // Damage types
        if (damageData.Type == DamageType.Piercing)
            damage -= Defense / 4;
        else if (damageData.Type == DamageType.Bludgeoning)
            damage -= Defense / 2;
        else if (damageData.Type == DamageType.Slashing) damage -= Defense;

        if (damage < 1)
            damage = 1;

        if (damageData.IsCritical)
        {
            damage *= 2;
            knockback *= 2;
            FloatingText.Create(damage.ToString(), Color.yellow, transform.position, 1.2f, 0.5f, 0.2f);
        }
        else
        {
            FloatingText.Create(damage.ToString(), Color.red, transform.position, 1f, 0.5f, 0.2f);
        }

        CalculateKnockback(damageData.Source.transform.position, knockback);
        Health -= damage;
        if (Health <= 0) Die();
    }

    public virtual void Heal(float amount)
    {
        if (Health + amount > MaxHealth)
            Health = MaxHealth;
        else
            Health += amount;

        FloatingText.Create(amount.ToString(), Color.green, transform.position, 1f, 0.5f, 0.2f);
    }

    private void CalculateKnockback(Vector3 from, float distance)
    {
        KnockbackDirection = transform.position - from;
        KnockbackDirection.Normalize();
        KnockbackDirection *= distance;
    }

    // Actor death
    private protected virtual void Die()
    {
        Destroy(gameObject);
    }

    private protected virtual void Move()
    {
        if (Resistance >= 100)
        {
            KnockbackDirection = Vector3.zero;
        }
        else
        {
            var realResistance = Resistance / 100f;
            if (realResistance == 0) realResistance = 0.01f;
            KnockbackDirection = Vector3.Lerp(KnockbackDirection, Vector3.zero, realResistance);
        }

        MoveDelta += KnockbackDirection;
        RigidBody.velocity = MoveDelta;
    }

    private protected virtual void CalculateMovement()
    {
        MoveDelta = Target.transform.position - transform.position;
        MoveDelta.Normalize();
        MoveDelta *= MovementSpeed;
    }

    private protected virtual void LookAt(Vector3 lookTarget, ActorType actorType)
    {
        // Calculating position difference between the target and actor
        var posDif = lookTarget - transform.position;

        // Calculating the angle of the target relative to the actor
        var z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
        if (z < 0) z = 180 + (180 - Mathf.Abs(z));

        // Manipulating sprite to look at target
        if (z >= 45 && z < 135)
        {
            SpriteRenderer.sprite = BackSprite;
        }
        else if (z >= 135 && z < 225)
        {
            SpriteRenderer.sprite = SideSprite;
            SpriteRenderer.flipX = true;
        }
        else if (z >= 225 && z < 315)
        {
            SpriteRenderer.sprite = FrontSprite;
        }
        else
        {
            SpriteRenderer.sprite = SideSprite;
            SpriteRenderer.flipX = false;
        }
    }
}
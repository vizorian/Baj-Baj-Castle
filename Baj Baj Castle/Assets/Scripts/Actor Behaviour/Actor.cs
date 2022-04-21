using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public bool isActive;

    // attributes
    public float Health;
    public float MaxHealth;
    public float MovementSpeed;
    public int Defense;
    public int Resistance;


    // upgradeable attributes
    public int Strength;
    public int Agility;
    public int Intelligence;
    public int Luck;

    // range attributes
    public float InteractionRange;
    public float ReachRange;
    public float ViewRange;

    public Actor(int health,
                 int maxHealth,
                 int movementSpeed,
                 int defense,
                 int resistance,
                 int strength,
                 int agility,
                 int intelligence,
                 int luck,
                 float interactionRange,
                 float reachRange,
                 float viewRange)
    {
        Health = health;
        MaxHealth = maxHealth;
        MovementSpeed = movementSpeed;
        Defense = defense;
        Resistance = resistance;
        Strength = strength;
        Agility = agility;
        Intelligence = intelligence;
        Luck = luck;
        InteractionRange = interactionRange;
        ReachRange = reachRange;
        ViewRange = viewRange;
    }

    public Actor()
    {
    }

    private protected BoxCollider2D boxCollider;
    private protected SpriteRenderer spriteRenderer;
    private protected RaycastHit2D raycastHit;
    private protected InventorySystem inventory;

    public ActorType ActorType;
    public GameObject HandPrefab;
    public ActorHand Hand;

    private protected GameObject interactionObject;
    private protected GameObject target;
    private protected Vector3 moveDelta;
    private protected Vector3 knockbackDirection;
    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite SideSprite;

    private protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (HandPrefab != null)
        {
            Hand = Instantiate(HandPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform).GetComponent<ActorHand>();
            Hand.Init(ReachRange);
            Hand.UpdateCenterPosition(transform.position);
        }
    }

    // Take damage, called by weapons on collision
    private protected virtual void TakeDamage(DamageData damageData)
    {
        CalculateKnockback(damageData.Source.transform.position, damageData.Knockback);
        var damage = damageData.Amount;

        // Adjust damage based on if the weapon is flipped or not
        if (damageData.Type == DamageType.Piercing && damageData.Source.Hand.IsItemTurned)
        {
            damageData.Type = DamageType.Slashing;
        }
        else if (damageData.Type == DamageType.Slashing && damageData.Source.Hand.IsItemTurned)
        {
            damageData.Type = DamageType.Piercing;
        }

        // Damage types
        if (damageData.Type == DamageType.Piercing)
        {
            damage -= Defense / 4;
        }
        else if (damageData.Type == DamageType.Bludgeoning)
        {
            damage -= Defense / 2;
        }
        else if (damageData.Type == DamageType.Slashing)
        {
            damage -= Defense;
        }

        if (damage < 1)
            damage = 1;


        // TODO Resistances of multiple damage types?

        Health -= damage;
        if (damageData.IsCritical)
        {
            FloatingText.Create(damage.ToString(), Color.yellow, transform.position, 1.2f, 0.5f, 0.2f);
        }
        else
        {
            FloatingText.Create(damage.ToString(), Color.red, transform.position, 1f, 0.5f, 0.2f);
        }
        if (Health <= 0)
        {
            Die();
        }
    }

    // TODO use this for potions
    public virtual void Heal(float amount)
    {
        if (Health + amount > MaxHealth)
        {
            Health = MaxHealth;
        }
        else
        {
            Health += amount;
        }
    }

    // TODO improve this
    // HOW: check if new position is in collision
    private void CalculateKnockback(Vector3 from, float distance)
    {
        knockbackDirection = transform.position - from;
        knockbackDirection.Normalize();
        knockbackDirection *= distance;
    }

    // Actor death
    private protected virtual void Die()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Creates a box cast to check for collisions on both axis and moves the player if there are none
    /// </summary>
    private protected virtual void Move()
    {
        if (!isActive)
        {
            knockbackDirection = Vector3.zero;
            moveDelta = Vector3.zero;
        }


        var realResistance = 0.1f + Resistance / 100f;
        if (realResistance > 1)
        {
            knockbackDirection = Vector3.zero;
        }
        else
        {
            knockbackDirection = Vector3.Lerp(knockbackDirection, Vector3.zero, realResistance);
        }
        moveDelta += knockbackDirection;

        if (moveDelta != Vector3.zero)
        {
            // Checking for collision on X axis
            raycastHit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, new Vector2(moveDelta.x, 0),
                0.01f, LayerMask.GetMask("Actor", "Blocking"));

            if (raycastHit.collider == null)
            {
                // Applying movement on X axis
                transform.Translate(moveDelta.x, 0, 0);
            }

            // Checking for collision on Y axis
            raycastHit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, new Vector2(0, moveDelta.y),
                0.01f, LayerMask.GetMask("Actor", "Blocking"));

            if (raycastHit.collider == null)
            {
                // Applying movement on Y axis
                transform.Translate(0, moveDelta.y, 0);
            }
        }
    }

    private protected virtual void CalculateMovement()
    {
        moveDelta = target.transform.position - transform.position;
        moveDelta.Normalize();
        moveDelta *= MovementSpeed * Time.fixedDeltaTime;
    }

    private protected virtual void LookAt(Vector3 lookTarget, ActorType actorType)
    {
        // Calculating position difference between the target and actor
        Vector3 posDif = lookTarget - transform.position;

        // Out of range
        if (actorType != ActorType.Player)
        {
            if (posDif.magnitude > ViewRange)
            {
                target = null;
                return;
            }
        }

        // Calculating the angle of the target relative to the actor
        float z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
        if (z < 0) z = 180 + (180 - Mathf.Abs(z));

        // Manipulating sprite to look at target
        if (z >= 45 && z < 135)
            spriteRenderer.sprite = BackSprite;
        else if (z >= 135 && z < 225)
        {
            spriteRenderer.sprite = SideSprite;
            spriteRenderer.flipX = true;
        }
        else if (z >= 225 && z < 315)
            spriteRenderer.sprite = FrontSprite;
        else
        {
            spriteRenderer.sprite = SideSprite;
            spriteRenderer.flipX = false;
        }
    }
}

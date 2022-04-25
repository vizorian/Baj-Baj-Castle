using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO implement collisons
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

    private protected Rigidbody2D rigidBody;
    private protected BoxCollider2D boxCollider;
    private protected SpriteRenderer spriteRenderer;
    private protected RaycastHit2D raycastHit;
    private protected ContactFilter2D contactFilter;
    private protected List<Collider2D> _hits = new List<Collider2D>();
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

    private protected void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Actor"));
        if (HandPrefab != null)
        {
            Hand = Instantiate(HandPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform).GetComponent<ActorHand>();
            Hand.Init(ReachRange);
            Hand.UpdateCenterPosition(transform.position);
        }
    }

    private protected virtual void FixedUpdate()
    {
        // Handling collisions
        boxCollider.OverlapCollider(contactFilter, _hits);
        for (int i = 0; i < _hits.Count; i++)
        {
            OnCollide(_hits[i]);
            _hits[i] = null;
        }
    }

    private protected virtual void OnCollide(Collider2D collider)
    {
        // var Damage = 10;
        // collider.gameObject.SendMessage("TakeDamage", Damage);
        Debug.Log(gameObject.name + " collided with " + collider.name);
    }

    // Take damage, called by weapons on collision
    private protected virtual void TakeDamage(DamageData damageData)
    {
        Debug.Log(gameObject.name + " took taking damage");
        var knockback = damageData.Knockback;
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
        if (Health <= 0)
        {
            Die();
        }
    }

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

        FloatingText.Create(amount.ToString(), Color.green, transform.position, 1f, 0.5f, 0.2f);
    }

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
        if (Resistance >= 100)
        {
            knockbackDirection = Vector3.zero;
        }
        else
        {
            var realResistance = 0.1f + Resistance / 100f;
            knockbackDirection = Vector3.Lerp(knockbackDirection, Vector3.zero, realResistance);
        }

        moveDelta += knockbackDirection;
        rigidBody.velocity = moveDelta;
    }

    private protected virtual void CalculateMovement()
    {
        moveDelta = target.transform.position - transform.position;
        moveDelta.Normalize();
        moveDelta *= MovementSpeed;
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

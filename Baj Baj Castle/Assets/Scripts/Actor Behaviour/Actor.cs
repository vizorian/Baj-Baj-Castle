using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    // Attributes
    public int Health;
    public int MaxHealth;
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

    private protected BoxCollider2D _boxCollider;
    private protected SpriteRenderer _spriteRenderer;
    private protected RaycastHit2D raycastHit;
    private protected InventorySystem inventory;

    public ActorType actorType;
    public GameObject HandPrefab;
    private protected ActorHand _hand;

    private protected GameObject interactionObject;
    private protected GameObject target;

    private protected Vector3 moveDelta;

    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite SideSprite;

    private protected virtual void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _hand = Instantiate(HandPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform).GetComponent<ActorHand>();
        _hand.Init(ReachRange);
        _hand.UpdateCenterPosition(transform.position);
    }

    // Take damage, called by other actors on collision
    private protected virtual void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Death();
        }
    }

    // Death, called by TakeDamage
    private protected virtual void Death()
    {
    }

    // ActorHand commands
    private protected virtual void UpdateHeldItem(InventoryItem item)
    {
        _hand.SetHeldItem(item);
    }

    private protected virtual void ClearHeldItem()
    {
        _hand.ClearHeldItem();
    }

    private protected virtual void TurnHeldItem()
    {
        _hand.TurnHeldItem();
    }

    /// <summary>
    /// Creates a box cast to check for collisions on both axis and moves the player if there are none
    /// </summary>
    private protected virtual void Move()
    {
        // Checking for collision on X axis
        raycastHit = Physics2D.BoxCast(transform.position, _boxCollider.size, 0, new Vector2(moveDelta.x, 0),
            0.01f, LayerMask.GetMask("Actor", "Blocking"));

        if (raycastHit.collider == null)
        {
            // Applying movement on X axis
            transform.Translate(moveDelta.x * Time.deltaTime * MovementSpeed, 0, 0);
        }

        // Checking for collision on Y axis
        raycastHit = Physics2D.BoxCast(transform.position, _boxCollider.size, 0, new Vector2(0, moveDelta.y),
            0.01f, LayerMask.GetMask("Actor", "Blocking"));

        if (raycastHit.collider == null)
        {
            // Applying movement on Y axis
            transform.Translate(0, moveDelta.y * Time.deltaTime * MovementSpeed, 0);
        }
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
            _spriteRenderer.sprite = BackSprite;
        else if (z >= 135 && z < 225)
        {
            _spriteRenderer.sprite = SideSprite;
            _spriteRenderer.flipX = true;
        }
        else if (z >= 225 && z < 315)
            _spriteRenderer.sprite = FrontSprite;
        else
        {
            _spriteRenderer.sprite = SideSprite;
            _spriteRenderer.flipX = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private protected BoxCollider2D _boxCollider;
    private protected SpriteRenderer _spriteRenderer;
    private protected RaycastHit2D raycastHit;
    private protected Inventory inventory;

    public enum ActorType
    {
        Enemy,
        Player
    };

    public ActorType actorType;

    private protected GameObject interactionObject;
    private protected GameObject target;

    private protected Vector3 moveDelta;

    public float MovementSpeed = 0.5f;
    public float InteractionRange = 0.15f;
    public float ViewRange = 1f;

    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite SideSprite;

    protected virtual void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void OnDrawGizmos()
    {
        
    }

    /// <summary>
    /// Creates a box cast to check for collisions on both axis and moves the player if there are none
    /// </summary>
    protected private virtual void Move()
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

    protected private virtual void LookAt(Vector3 lookTarget, ActorType actorType)
    {
        // Calculating position difference between the target and actor
        Vector3 posDif = lookTarget - transform.position;

        // Out of range
        if(actorType == ActorType.Enemy)
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

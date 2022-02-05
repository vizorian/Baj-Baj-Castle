using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;

    private Vector3 moveDelta;
    private RaycastHit2D raycastHit;
    private GameObject interactionObject;

    public float MovementSpeed = 0.5f;
    public float InteractionRange = 0.05f;

    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite SideSprite;

    void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        ProcessInputs();
        LookAtMouse();
        FindAndSetInteractableObject();
        if (interactionObject != null)
            interactionObject.SendMessage("ShowHighlight", gameObject);
    }

    private void FindAndSetInteractableObject()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Object");

        List<GameObject> interactableObjects = new List<GameObject>();

        foreach (GameObject obj in objects)
        {
            if(Vector3.Distance(transform.position, obj.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)) <= InteractionRange)
            {
                interactableObjects.Add(obj);
            }
            else
            {
                interactableObjects.Remove(obj);
            }
        }

        if(interactableObjects.Count == 0)
        {
            interactionObject = null;
            return;
        }

        foreach (GameObject obj in interactableObjects)
        {
            
            if(interactionObject != null)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < Vector3.Distance(transform.position, interactionObject.transform.position))
                {
                    interactionObject = obj;
                }
            }
            else
            {
                interactionObject = obj;
            }
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Creates a box cast to check for collisions on both axis and moves the player if there are none
    /// </summary>
    private void Move()
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

    /// <summary>
    /// Processes the incoming inputs
    /// </summary>
    private void ProcessInputs()
    {
        // Getting inputs
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Recalculating the movement direction
        moveDelta = new Vector2(x, y).normalized;
    }

    /// <summary>
    /// Updates the sprite to face the mouse in 4 directions
    /// </summary>
    private void LookAtMouse()
    {
        // Getting mouse position in world
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Calculating position difference between the mouse and player
        Vector3 posDif = mousePos - transform.position;

        // Calculating the angle of the mouse relative to the player
        float z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
        if (z < 0) z = 180 + (180 - Mathf.Abs(z));

        if (z >= 45 && z < 135)
        {
            _spriteRenderer.sprite = BackSprite;
        }else if(z >= 135 && z < 225)
        {
            _spriteRenderer.sprite = SideSprite;
            _spriteRenderer.flipX = true;
        }
        else if (z >= 225 && z < 315)
        {
            _spriteRenderer.sprite = FrontSprite;
        }
        else
        {
            _spriteRenderer.sprite = SideSprite;
            _spriteRenderer.flipX = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (interactionObject == null) Gizmos.color = Color.yellow;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, InteractionRange);
    }
}

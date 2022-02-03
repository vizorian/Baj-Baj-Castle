using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 moveDelta;
    private RaycastHit2D raycastHit;
    public float speed = 0.5f;

    private bool changedDirection = false;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        // Getting inputs
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Checking if the player has moved in another direction
        if (x != moveDelta.x)
        {
            changedDirection = true;
        }
        else
        { 
            changedDirection = false; 
        }

        // Recalculating the move delta
        moveDelta = new Vector3(x, y);

        // Flipping the sprite based on the direction
        if (x < 0 && changedDirection)
        {
            spriteRenderer.flipX = true;
        }
        else if (x > 0 && changedDirection)
        {
            spriteRenderer.flipX = false;
        }

        // Checking for collision on X axis
        raycastHit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, new Vector2(moveDelta.x, 0),
            0.01f, LayerMask.GetMask("Actor", "Blocking"));
        if (raycastHit.collider == null)
        {
            // Applying movement on X axis
            transform.Translate(moveDelta.x * Time.deltaTime * speed, 0, 0);
        }

        // Checking for collision on Y axis
        raycastHit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, new Vector2(0, moveDelta.y),
            0.01f, LayerMask.GetMask("Actor", "Blocking"));
        if (raycastHit.collider == null)
        {
            // Applying movement on Y axis
            transform.Translate(0, moveDelta.y * Time.deltaTime * speed, 0);
        }
    }
}

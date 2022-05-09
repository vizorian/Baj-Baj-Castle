using JetBrains.Annotations;
using UnityEngine;

public class Interactable : Collidable
{
    public float Health;

    private protected LineRenderer LineRenderer;
    private protected SpriteRenderer SpriteRenderer;
    private protected bool Collisions;
    private protected bool Up, Right, Down, Left;

    private protected override void Awake()
    {
        base.Awake();
        SpriteRenderer = GetComponent<SpriteRenderer>();

        if (LineRenderer == null)
            CreateLineRenderer();
    }

    private protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Collisions) LineRenderer.positionCount = 0;
    }

    // Handle collision
    private protected override void OnCollide(Collider2D otherCollider)
    {
        Collisions = true;

        if (otherCollider.tag == "Player")
            DrawHighlight(otherCollider.gameObject);

        Collisions = false;
    }

    // Draw directional highlight
    protected virtual void DrawHighlight(GameObject obj)
    {
        var direction = obj.transform.position - transform.position;

        var forward = Vector3.Dot(direction, transform.up);
        var side = Vector3.Dot(direction, transform.right);

        var vertices = GetVertexPositions(gameObject);

        var topVert = new Vector3[2];
        var rightVert = new Vector3[2];
        var bottomVert = new Vector3[2];
        var leftVert = new Vector3[2];

        topVert[0] = vertices[0];
        topVert[1] = vertices[1];

        rightVert[0] = vertices[1];
        rightVert[1] = vertices[3];

        bottomVert[0] = vertices[2];
        bottomVert[1] = vertices[3];

        leftVert[0] = vertices[0];
        leftVert[1] = vertices[2];

        LineRenderer.positionCount = 2;

        Up = false;
        Right = false;
        Down = false;
        Left = false;

        if (Mathf.Abs(forward) > Mathf.Abs(side))
        {
            if (forward > 0)
            {
                LineRenderer.SetPositions(topVert);
                Down = true;
            }
            else
            {
                LineRenderer.SetPositions(bottomVert);
                Up = true;
            }
        }
        else
        {
            if (side > 0)
            {
                LineRenderer.SetPositions(rightVert);
                Right = true;
            }
            else
            {
                LineRenderer.SetPositions(leftVert);
                Left = true;
            }
        }
    }

    // Draw complete highlight
    protected virtual void DrawHighlightFull(GameObject obj)
    {
        if (LineRenderer == null) return;

        var vertices = GetVertexPositions(gameObject);
        var newVertices = new Vector3[vertices.Length + 1];
        newVertices[0] = vertices[0];
        newVertices[1] = vertices[1];
        newVertices[2] = vertices[3];
        newVertices[3] = vertices[2];
        newVertices[4] = vertices[0];

        LineRenderer.positionCount = 5;
        LineRenderer.SetPositions(newVertices);
    }

    // Create line renderer component
    private protected void CreateLineRenderer()
    {
        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.positionCount = 0;
        LineRenderer.startWidth = 0.005f;
        LineRenderer.endWidth = 0.005f;
        LineRenderer.material = SpriteRenderer.material;
        LineRenderer.sortingLayerName = "Render";
    }

    // Handle interaction
    [UsedImplicitly]
    private protected virtual void OnInteraction()
    {
        Debug.Log("Interacted with object");
    }

    // Get vertex positions
    private Vector3[] GetVertexPositions(GameObject obj)
    {
        var vertices = new Vector3[4];
        if (SpriteRenderer == null) return vertices;

        var thisMatrix = obj.transform.localToWorldMatrix;
        var storedRotation = obj.transform.rotation;
        obj.transform.rotation = Quaternion.identity;
        var extents = SpriteRenderer.bounds.extents;

        vertices[0] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, 0));
        vertices[1] = thisMatrix.MultiplyPoint3x4(extents);
        vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, 0));
        vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, 0));

        obj.transform.rotation = storedRotation;
        return vertices;
    }

    // Receive and process damage data
    [UsedImplicitly]
    private protected virtual void TakeDamage(DamageData damageData)
    {
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
            damage /= 4;
        else if (damageData.Type == DamageType.Slashing) damage /= 2;

        if (damage < 1)
            damage = 1;


        Health -= damage;
        FloatingText.Create(damage.ToString(), Color.grey, transform.position, 1f, 0.5f, 0.2f);
        if (Health <= 0) Die();
    }

    // Death logic
    private protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
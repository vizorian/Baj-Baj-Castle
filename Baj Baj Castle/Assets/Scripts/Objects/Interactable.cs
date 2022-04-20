using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : Collidable
{
    // Attributes
    public float Health;

    private protected LineRenderer _lineRenderer;
    private protected SpriteRenderer _spriteRenderer;
    private protected Color _highlightColor = Color.white;

    private protected bool collisions = false;
    private protected bool up, right, down, left = false;

    private protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_lineRenderer == null)
            CreateLineRenderer();
    }

    private protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!collisions)
            _lineRenderer.positionCount = 0;
    }

    private protected override void OnCollide(Collider2D collider)
    {
        collisions = true;

        if (collider.tag == "Player")
            DrawHighlight(collider.gameObject);

        collisions = false;
    }

    protected virtual void DrawHighlight(GameObject obj)
    {
        Vector3 direction = obj.transform.position - transform.position;

        var forward = Vector3.Dot(direction, transform.up);
        var side = Vector3.Dot(direction, transform.right);

        var vertices = GetVertexPositions(gameObject);

        Vector3[] topVert = new Vector3[2];
        Vector3[] rightVert = new Vector3[2];
        Vector3[] bottomVert = new Vector3[2];
        Vector3[] leftVert = new Vector3[2];

        topVert[0] = vertices[0];
        topVert[1] = vertices[1];

        rightVert[0] = vertices[1];
        rightVert[1] = vertices[3];

        bottomVert[0] = vertices[2];
        bottomVert[1] = vertices[3];

        leftVert[0] = vertices[0];
        leftVert[1] = vertices[2];

        _lineRenderer.positionCount = 2;

        up = false;
        right = false;
        down = false;
        left = false;

        if (Mathf.Abs(forward) > Mathf.Abs(side))
        {
            if (forward > 0)
            {
                _lineRenderer.SetPositions(topVert);
                down = true;
            }
            else
            {
                _lineRenderer.SetPositions(bottomVert);
                up = true;
            }
        }
        else
        {
            if (side > 0)
            {
                _lineRenderer.SetPositions(rightVert);
                right = true;
            }
            else
            {
                _lineRenderer.SetPositions(leftVert);
                left = true;
            }
        }
    }

    protected virtual void DrawHighlightFull(GameObject obj)
    {
        Vector3[] vertices = GetVertexPositions(gameObject);
        Vector3[] newVertices = new Vector3[vertices.Length + 1];
        newVertices[0] = vertices[0];
        newVertices[1] = vertices[1];
        newVertices[2] = vertices[3];
        newVertices[3] = vertices[2];
        newVertices[4] = vertices[0];

        _lineRenderer.positionCount = 5;
        _lineRenderer.SetPositions(newVertices);
    }

    private protected void CreateLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _lineRenderer.startWidth = 0.005f;
        _lineRenderer.endWidth = 0.005f;
        _lineRenderer.material = _spriteRenderer.material;
        _lineRenderer.sortingLayerName = "Render";
    }

    private protected virtual void OnInteraction()
    {
        Debug.Log("Interacted with object");
    }

    private Vector3[] GetVertexPositions(GameObject obj)
    {
        var vertices = new Vector3[4];
        var thisMatrix = obj.transform.localToWorldMatrix;
        var storedRotation = obj.transform.rotation;
        obj.transform.rotation = Quaternion.identity;

        var extents = _spriteRenderer.bounds.extents;

        vertices[0] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, 0));
        vertices[1] = thisMatrix.MultiplyPoint3x4(extents);
        vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, 0));
        vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, 0));

        obj.transform.rotation = storedRotation;
        return vertices;
    }

    private protected virtual void TakeDamage(DamageData damageData)
    {
        var damage = damageData.Amount;

        // Adjust damage based on if the weapon is flipped or not
        if (damageData.Type == DamageType.Piercing && damageData.Source.Hand.IsTurned)
        {
            damageData.Type = DamageType.Slashing;
        }
        else if (damageData.Type == DamageType.Slashing && damageData.Source.Hand.IsTurned)
        {
            damageData.Type = DamageType.Piercing;
        }

        // Damage types
        if (damageData.Type == DamageType.Piercing)
        {
            damage /= 4;
        }
        else if (damageData.Type == DamageType.Slashing)
        {
            damage /= 4;
        }

        if (damage < 1)
            damage = 1;


        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    private protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

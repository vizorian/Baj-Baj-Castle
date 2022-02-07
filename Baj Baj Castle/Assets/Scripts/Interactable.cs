using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : Collidable
{
    private LineRenderer _lineRenderer;

    private protected SpriteRenderer _spriteRenderer;
    private protected Color _highlightColor = Color.white;

    private protected bool drawHighlights = false;
    private protected bool cleared = false;
    private protected bool isActive = true;
    private protected bool up, right, down, left = false;

    private protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private protected override void Update()
    {
        if (_lineRenderer != null && !drawHighlights && !cleared)
        {
            _lineRenderer.positionCount = 0;
            cleared = true;
        }
    }

    private protected override void OnCollide(Collider2D collider)
    {
        if (collider.tag == "Player" && isActive)
            drawHighlights = true;

        if (drawHighlights)
        {
            DrawHighlight(collider.gameObject);
            drawHighlights = false;
        }
    }

    protected virtual void DrawHighlight(GameObject obj)
    {
        if(_lineRenderer == null)
            CreateLineRenderer();
        
        _lineRenderer.positionCount = 2;
        cleared = false;

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

        if (Mathf.Abs(forward) > Mathf.Abs(side))
        {
            if (forward > 0)
            {
                _lineRenderer.SetPositions(topVert);
                up = false;
                right = false;
                down = true;
                left = false;
            }
            else
            {
                _lineRenderer.SetPositions(bottomVert);
                up = true;
                right = false;
                down = false;
                left = false;
            }
        }
        else
        {
            if (side > 0)
            {
                _lineRenderer.SetPositions(rightVert);
                up = false;
                right = true;
                down = false;
                left = false;
            }
            else
            {
                _lineRenderer.SetPositions(leftVert);
                up = false;
                right = false;
                down = false;
                left = true;
            }
        }
    }

    protected virtual void DrawHighlightFull(GameObject obj)
    {
        if (_lineRenderer == null)
            CreateLineRenderer();

        _lineRenderer.positionCount = 5;
        cleared = false;

        Vector3[] vertices = GetVertexPositions(gameObject);
        Vector3[] newVertices = new Vector3[vertices.Length+1];
        newVertices[0] = vertices[0];
        newVertices[1] = vertices[1];
        newVertices[2] = vertices[3];
        newVertices[3] = vertices[2];
        newVertices[4] = vertices[0];

        _lineRenderer.SetPositions(newVertices);
    }

    private void CreateLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
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
}

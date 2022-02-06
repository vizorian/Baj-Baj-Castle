using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;

    public Color HighlightColor = Color.white;

    public Sprite StandardSprite;
    public Sprite DownSprite;
    public Sprite UpSprite;
    public Sprite SideSprite;
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        
    }

    private void ShowHighlight(GameObject gameObject)
    {
        Vector3 direction = gameObject.transform.position - transform.position;

        var forward = Vector3.Dot(direction, transform.up);
        var side = Vector3.Dot(direction, transform.right);

        var vertices = GetVertexPositions(this.gameObject);

        if (Mathf.Abs(forward) > Mathf.Abs(side))
        {
            if (forward > 0)
            {
                Debug.Log("Back");
                Debug.DrawLine(vertices[0], vertices[1]);
            }
            else
            {
                Debug.Log("Front");
                Debug.DrawLine(vertices[3], vertices[2]);
            }
        }
        else
        {
            if (side > 0)
            {
                Debug.Log("Right");
                Debug.DrawLine(vertices[1], vertices[3]);
            }
            else
            {
                Debug.Log("Left");
                Debug.DrawLine(vertices[0], vertices[2]);
            }
        }


        //if (z >= 45 && z < 135)
        //{
        //    _spriteRenderer.sprite = BackSprite;
        //}
        //else if (z >= 135 && z < 225)
        //{
        //    _spriteRenderer.sprite = SideSprite;
        //    _spriteRenderer.flipX = true;
        //}
        //else if (z >= 225 && z < 315)
        //{
        //    _spriteRenderer.sprite = FrontSprite;
        //}
        //else
        //{
        //    _spriteRenderer.sprite = SideSprite;
        //    _spriteRenderer.flipX = false;
        //}
    }

    private Vector3[] GetVertexPositions(GameObject gameObject)
    {
        var vertices = new Vector3[4];
        var thisMatrix = gameObject.transform.localToWorldMatrix;
        var storedRotation = gameObject.transform.rotation;
        gameObject.transform.rotation = Quaternion.identity;

        var extents = _spriteRenderer.bounds.extents;

        vertices[0] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, 0));
        vertices[1] = thisMatrix.MultiplyPoint3x4(extents);
        vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, 0));
        vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, 0));

        gameObject.transform.rotation = storedRotation;
        return vertices;
    }
}

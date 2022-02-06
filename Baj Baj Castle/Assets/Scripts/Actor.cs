using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private protected BoxCollider2D _boxCollider;
    private protected SpriteRenderer _spriteRenderer;

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
}

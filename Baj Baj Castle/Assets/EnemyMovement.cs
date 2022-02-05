using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;

    private Vector3 moveDelta;
    private RaycastHit2D raycastHit;
    private GameObject target;

    public float MovementSpeed = 0.5f;
    public float ViewRange = 0.2f;

    public Sprite FrontSprite;
    public Sprite BackSprite;
    public Sprite SideSprite;

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null)
            FindAndSetTarget();
        else
            LookAtTarget();
    }

    private void FindAndSetTarget()
    {
        var potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        foreach(var t in potentialTargets)
        {
            if(Vector3.Distance(transform.position, t.transform.position) <= ViewRange)
            {
                target = t;
                return;
            }
        }
    }

    private void LookAtTarget()
    {
        // Getting target position in world
        Vector3 targetPos = target.transform.position;
        // Calculating position difference between the target and actor
        Vector3 posDif = targetPos - transform.position;

        if(posDif.magnitude > ViewRange)
        {
            target = null;
            return;
        }

        // Calculating the angle of the target relative to the actor
        float z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
        if (z < 0) z = 180 + (180 - Mathf.Abs(z));

        if (z >= 45 && z < 135)
        {
            _spriteRenderer.sprite = BackSprite;
        }
        else if (z >= 135 && z < 225)
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
        if (target == null) Gizmos.color = Color.yellow;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ViewRange);
    }
}

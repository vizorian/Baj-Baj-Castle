using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collidable : MonoBehaviour
{
    private protected ContactFilter2D contactFilter;
    private protected BoxCollider2D boxCollider;
    private protected List<Collider2D> _hits = new List<Collider2D>();

    private protected virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        contactFilter = new ContactFilter2D();
    }

    private protected virtual void FixedUpdate()
    {
        // Handling collisions
        boxCollider.OverlapCollider(contactFilter, _hits);
        for (int i = 0; i < _hits.Count; i++)
        {
            OnCollide(_hits[i]);
            _hits[i] = null;
        }
    }

    // Handles collision
    private protected virtual void OnCollide(Collider2D collider)
    {
        Debug.Log(collider.name);
    }
}


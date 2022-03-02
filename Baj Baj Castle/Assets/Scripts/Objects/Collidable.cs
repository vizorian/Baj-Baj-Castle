using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collidable : MonoBehaviour
{
    public ContactFilter2D ContactFilter;
    private BoxCollider2D _boxCollider;
    private protected List<Collider2D> _hits = new List<Collider2D>();

    private protected virtual void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    private protected virtual void FixedUpdate()
    {
        // Handling collisions
        _boxCollider.OverlapCollider(ContactFilter, _hits);
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


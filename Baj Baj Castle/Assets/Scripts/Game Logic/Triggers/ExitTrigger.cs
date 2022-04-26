using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : Collidable
{
    public bool IsActive = true;
    private bool isUsed = true;
    
    private override protected void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private override protected void OnCollide(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player" && isUsed && IsActive)
        {
            isUsed = false;
            if (GameManager.Instance.Level == GameManager.Instance.MaxLevels)
            {
                GameManager.Instance.Victory();
            }
            else
            {
                GameManager.Instance.NextLevel();
            }
        }
    }
}

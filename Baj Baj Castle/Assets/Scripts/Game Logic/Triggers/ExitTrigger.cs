using UnityEngine;

public class ExitTrigger : Collidable
{
    public bool IsActive = true;
    private bool isUsed = true;

    private protected override void Awake()
    {
        BoxCollider = GetComponent<BoxCollider2D>();
        if (BoxCollider == null) BoxCollider = gameObject.AddComponent<BoxCollider2D>();
    }

    // Handle collision
    private protected override void OnCollide(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.tag == "Player" && isUsed && IsActive)
        {
            isUsed = false;
            if (GameManager.Instance.Level == GameManager.Instance.MaxLevels)
                GameManager.Instance.Victory();
            else
                GameManager.Instance.NextLevel();
        }
    }
}
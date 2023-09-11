namespace Objects;

public class Pickupable : Interactable
{
    public InventoryItemData ItemData;

    // Handle collision
    private protected override void OnCollide(Collider2D otherCollider)
    {
        Collisions = true;

        if (otherCollider.tag == "Player") DrawHighlightFull(otherCollider.gameObject);

        Collisions = false;
    }

    // Handle interaction
    private protected override void OnInteraction()
    {
        if (InventorySystem.Instance.Add(ItemData))
            Destroy(gameObject);
        else
            FloatingText.Create("Inventory is full", Color.red, transform.position, 1, 0.5f, 0.5f);
    }
}
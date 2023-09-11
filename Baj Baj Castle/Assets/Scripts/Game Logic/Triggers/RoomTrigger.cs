namespace Game_Logic.Triggers;

public class RoomTrigger : Collidable
{
    public ExitTrigger Exit;
    public Room ParentRoom; // room to trigger

    private protected override void Awake()
    {
        BoxCollider = GetComponent<BoxCollider2D>();
        ContactFilter = new ContactFilter2D();
        ContactFilter.SetLayerMask(LayerMask.GetMask("Actor"));
    }

    [UsedImplicitly]
    private void Update()
    {
        if (!ParentRoom.IsCleared)
            if (Exit != null)
                Exit.IsActive = false;
        if (ParentRoom.IsActive)
            if (ParentRoom.Actors.Count(a => a != null) <= 0) // if any ParentRoom.Actors are dead
            {
                ParentRoom.UnlockDoors();
                ParentRoom.IsCleared = true;
                ParentRoom.IsActive = false;
                if (Exit != null) Exit.IsActive = true;
                Destroy(gameObject);
            }
    }

    // Handle collision
    private protected override void OnCollide(Collider2D otherCollider)
    {
        if (otherCollider.tag == "Player")
            if (!ParentRoom.IsTriggered)
                ParentRoom.Trigger();
    }
}
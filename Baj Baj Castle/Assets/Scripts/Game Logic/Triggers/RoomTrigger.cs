using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class RoomTrigger : Collidable
{
    public ExitTrigger Exit;
    public Room ParentRoom; // room to trigger

    [UsedImplicitly]
    private void Update()
    {
        if (!ParentRoom.IsCleared)
            if (Exit != null)
                Exit.IsActive = false;
        if (ParentRoom.IsActive)
            // check if any ParentRoom.Actors are alive
            if (ParentRoom.Actors.Count(a => a != null) <= 0)
            {
                ParentRoom.UnlockDoors();
                ParentRoom.IsCleared = true;
                ParentRoom.IsActive = false;
                if (Exit != null) Exit.IsActive = true;
                Destroy(gameObject);
            }
    }

    private protected override void Awake()
    {
        BoxCollider = GetComponent<BoxCollider2D>();
        ContactFilter = new ContactFilter2D();
        ContactFilter.SetLayerMask(LayerMask.GetMask("Actor"));
    }

    private protected override void OnCollide(Collider2D otherCollider)
    {
        if (otherCollider.tag == "Player")
            if (!ParentRoom.IsTriggered)
                ParentRoom.Trigger();
    }
}
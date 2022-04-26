using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomTrigger : Collidable
{
    public Room ParentRoom; // room to trigger
    public ExitTrigger Exit;

    private void Update()
    {
        if (!ParentRoom.IsCleared)
        {
            if (Exit != null)
            {
                Exit.IsActive = false;
            }
        }
        if (ParentRoom.IsActive)
        {
            Debug.Log(ParentRoom.Actors.Count(a => a != null));
            // check if any ParentRoom.Actors are alive
            if (ParentRoom.Actors.Count(a => a != null) <= 0)
            {
                Debug.Log("Cleared room");
                ParentRoom.UnlockDoors();
                ParentRoom.IsCleared = true;
                ParentRoom.IsActive = false;
                if (Exit != null)
                {
                    Exit.IsActive = true;
                }
                Destroy(gameObject);
            }
        }
    }

    private protected override void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Actor"));
    }

    private protected override void OnCollide(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            if (!ParentRoom.IsTriggered)
            {
                ParentRoom.Trigger();
            }
        }
    }
}

namespace Actor_Behaviour;

public class ActorHand : MonoBehaviour
{
    public bool HoldingItem;
    public bool IsFreezingHand;
    public bool IsItemTurned;
    public float Velocity;
    public float HandSpeed = 1f;
    public float HandRange;
    public ItemType HeldItemType;
    private Vector3 bodyPosition;

    private bool handRangeSaved;
    private InventoryItem heldItem;
    private Vector3 heldItemHandlePosition;
    private GameObject itemObject;
    private bool newSelection;
    private float oldHandRange;

    [UsedImplicitly]
    private void Update()
    {
        UpdateHeldItem();
        UpdateVerticalRendering();
        if (!IsFreezingHand)
            UpdateHorizontalRendering();
    }

    // Hand initialization called from actor
    public void Init(float handRange)
    {
        HandRange = handRange;
    }

    // Flips the hand and held item based on horizontal position
    private void UpdateHorizontalRendering()
    {
        transform.localScale = Vector2.Dot(transform.parent.transform.right, transform.position - bodyPosition) >
                               0
            ? new Vector2(1, 1)
            : new Vector2(-1, 1);
    }

    // Item use logic
    public void UseHeldItem()
    {
        if (itemObject.GetComponent<Item>().Use(transform.parent.gameObject.GetComponent<Actor>()))
            InventorySystem.Instance.Remove(heldItem.Data);
    }

    // Updates rendering for the hand and held item based on vertical position
    private void UpdateVerticalRendering()
    {
        var handRenderer = transform.GetComponent<SpriteRenderer>();
        SpriteRenderer itemRenderer = null;

        if (heldItem != null)
            itemRenderer = itemObject.GetComponent<SpriteRenderer>();

        if (Vector2.Dot(transform.parent.transform.up, transform.position - bodyPosition) > 0) // If above actor
        {
            handRenderer.sortingLayerName = "Actor";
            if (itemRenderer != null)
                itemRenderer.sortingLayerName = "Actor";
        }
        else // If below actor
        {
            handRenderer.sortingLayerName = "Hand";
            if (itemRenderer != null)
                itemRenderer.sortingLayerName = "Hand";
        }
    }

    // Creates a new instance of the held item if there needs to be one
    private void UpdateHeldItem()
    {
        HoldingItem = true;
        if (newSelection || heldItem == null)
        {
            Destroy(itemObject);

            if (heldItem != null && newSelection)
            {
                InstantiateHeldItem();
                if (!handRangeSaved)
                {
                    oldHandRange = HandRange;
                    handRangeSaved = true;
                }

                UpdateHandAttributes(heldItem);
            }
        }
    }

    // Turns currently held item 90 degrees
    public void TurnHeldItem()
    {
        if (heldItem == null)
            return;

        if (!IsItemTurned)
        {
            itemObject.transform.Rotate(0, 0, 90);
            IsItemTurned = true;
        }
        else
        {
            itemObject.transform.Rotate(0, 0, -90);
            IsItemTurned = false;
        }

        // Realigns the hand and held item
        RealignHeldItem();
    }

    // Creates a new instance of the held item
    private void InstantiateHeldItem()
    {
        // Prefab preparation for actor use
        itemObject = Instantiate(heldItem.Data.Prefab, transform);
        itemObject.tag = "Untagged";
        Destroy(itemObject.GetComponent<Pickupable>());
        Destroy(itemObject.GetComponent<Collider2D>());

        itemObject.GetComponent<SpriteRenderer>().sortingLayerName = "Hand";
        itemObject.GetComponent<SpriteRenderer>().sortingOrder = 0;

        var item = itemObject.AddComponent<Item>();
        var properties = heldItem.Data.ItemProperties;
        HeldItemType = heldItem.Data.ItemType;
        item.Type = heldItem.Data.ItemType;
        item.Damage = properties.Damage;
        item.DamageType = properties.DamageType;
        item.CriticalChance = properties.CriticalChance;
        item.Speed = properties.Speed;
        item.Range = properties.Range;
        item.Knockback = properties.Knockback;
        item.Cooldown = properties.Cooldown;
        item.CooldownTimer = item.Cooldown;

        AlignHeldItem();

        newSelection = false;
    }

    // Moves the item so that the handle point is on the hand
    private void AlignHeldItem()
    {
        // Works for no flip
        var offsetX = Mathf.Abs(heldItemHandlePosition.x);
        var offsetY = Mathf.Abs(heldItemHandlePosition.y);

        var localDestination = new Vector3(offsetX, offsetY);
        itemObject.transform.localPosition = localDestination;
    }

    // Realigns the item after a flip
    private void RealignHeldItem()
    {
        var newPos = new Vector2(-itemObject.transform.localPosition.y, -itemObject.transform.localPosition.x);
        itemObject.transform.localPosition = newPos;
    }

    // Sets held item to new item
    public void SetHeldItem(InventoryItem item)
    {
        heldItem = item;

        if (item.Data.Prefab.transform.childCount != 0)
        {
            var childPos = item.Data.Prefab.transform.GetChild(0).gameObject.transform.localPosition;
            heldItemHandlePosition = new Vector3(childPos.x, childPos.y);
        }
        else
        {
            heldItemHandlePosition = Vector3.zero;
        }

        IsItemTurned = false;
        newSelection = true;
    }

    // Updates the hand attributes based on new item
    private void UpdateHandAttributes(InventoryItem item)
    {
        // Reset hand attributes
        ResetHandAttributes();
        // Reset hand position
        transform.position = bodyPosition;

        if (item.Data.ItemProperties.Speed != 0) HandSpeed = item.Data.ItemProperties.Speed;
        HandRange *= (100 + item.Data.ItemProperties.Range) / 100;
    }

    // Clears held item
    public void ClearHeldItem()
    {
        ResetHandAttributes();
        heldItem = null;
        HoldingItem = false;
        HeldItemType = ItemType.None;
    }

    // Resets hand attributes based on old data
    private void ResetHandAttributes()
    {
        HandSpeed = 1f;
        HandRange = oldHandRange;
    }

    // Updates actor center position, called by actor
    public void UpdateCenterPosition(Vector2 position)
    {
        bodyPosition = position;
    }

    // Looks towards the target position
    public void LookTowards(Vector3 lookTarget)
    {
        // Moves hand towards target
        Velocity = MoveTowards(lookTarget);

        //Rotates hand towards target
        if (!IsFreezingHand) // if hand not frozen
            RotateTowards(lookTarget);
    }

    // Move hand towards target within hand range
    private float MoveTowards(Vector3 target)
    {
        Vector2 targetPos = target;
        var oldPos = transform.position;

        // Getting step size
        var step = HandSpeed * Time.deltaTime;
        var velocity = step;
        // If mouse is out of range
        if (Vector2.Distance(bodyPosition, target) > HandRange)
        {
            // Move towards target
            transform.position = Vector2.MoveTowards(transform.position, targetPos, step);
            // Post step position is out of range
            if (Vector2.Distance(transform.position, bodyPosition) > HandRange)
            {
                // -------------------------------------------
                // Reset & Moving to border
                // -------------------------------------------

                // Reset position before step
                transform.position = oldPos;

                // Reduce step size by remaining distance to border
                var distanceToBorder = HandRange - Vector2.Distance(bodyPosition, oldPos);
                step -= distanceToBorder;

                // Move position the remaining distance to the border
                transform.position = Vector2.MoveTowards(transform.position, targetPos, distanceToBorder);

                // -------------------------------------------
                // Movement via range boundary
                // -------------------------------------------

                // Body to current position
                Vector2 bodyToCurrent = bodyPosition - transform.position;
                // Body to target position
                Vector2 bodyToTarget = bodyPosition - target;

                // Calculating rotation angle based on remaining step size
                // Use this to rotate from current position !!!
                var angleNextPoint = step / HandRange * Mathf.Rad2Deg;

                // Rotation of currentPos
                var currentRot = Quaternion.LookRotation(Vector3.forward, bodyToCurrent);

                // Rotation of targetPos
                var targetRot = Quaternion.LookRotation(Vector3.forward, bodyToTarget);

                // Rotate by nextPoint angle
                var angle = Quaternion.Angle(currentRot, targetRot);

                // Getting current Z rotation values
                var currentZ = currentRot.eulerAngles.z;
                var targetZ = targetRot.eulerAngles.z;

                // Converting euler to range of -180;180
                if (currentZ > 180)
                    currentZ -= 360;
                if (targetZ > 180)
                    targetZ -= 360;

                // Calculate rotation direction
                if ((currentZ >= 0 && targetZ >= 0) ||
                    (currentZ < 0 && targetZ < 0)) // If both are positive or negative
                {
                    if (currentZ > targetZ) // If negative rotation
                    {
                        angle *= -1;
                        angleNextPoint *= -1;
                    }
                }
                else if ((currentZ >= 0 && currentZ < 90) || currentZ <= -90)
                {
                    angle *= -1;
                    angleNextPoint *= -1;
                }

                Vector3 bodyToNewTarget;
                // Rotate the current rotation vector
                if (Quaternion.Angle(currentRot, targetRot) > Math.Abs(angleNextPoint)) // If out of step range
                    bodyToNewTarget = Quaternion.Euler(0, 0, angleNextPoint) * bodyToCurrent;
                else // If inside step range
                    bodyToNewTarget = Quaternion.Euler(0, 0, angle) * bodyToCurrent;

                // Apply final movement
                Vector2 newTargetPos = bodyPosition - bodyToNewTarget;
                velocity = Vector2.Distance(transform.position, newTargetPos);
                transform.position = Vector2.MoveTowards(transform.position, newTargetPos, Mathf.Infinity);
            }
        }
        else // Move to mouse within range
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, step);
        }

        return velocity;
    }

    // Rotates hand towards target
    private void RotateTowards(Vector3 target)
    {
        Vector2 direction = target - bodyPosition;
        transform.up = direction;
    }
}
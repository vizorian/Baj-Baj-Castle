using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Actor
{
    // Attributes
    public int Gold;
    public int StrengthUpgradeLevel;
    public int AgilityUpgradeLevel;
    public int IntelligenceUpgradeLevel;
    public int LuckUpgradeLevel;
    public int HealthUpgradeLevel;
    public int DefenseUpgradeLevel;

    private void Update()
    {
        ProcessInputs();
        CalculateMovement();
        LookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition), ActorType);


        FindInteractable();
        if (interactionObject != null)
            interactionObject.SendMessage("OnCollide", boxCollider);
    }

    private void FixedUpdate()
    {
        Hand.UpdateCenterPosition(transform.position);
        Hand.LookTowards(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Move();
    }

    /// <summary>
    /// Processes the incoming inputs
    /// </summary>
    private void ProcessInputs()
    {
        // Getting inputs
        float scrollWheelDelta = Input.GetAxisRaw("Mouse ScrollWheel");

        // Left click
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Hand.HoldingItem)
            {
                if (Hand.HeldItemType == ItemType.Consumable)
                {
                    Hand.UseHeldItem();
                }
            }
            Hand.IsFreezingHand = true;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Hand.IsFreezingHand = false;
        }

        // Interaction button
        if (Input.GetKeyDown(KeyCode.E) && interactionObject != null)
        {
            interactionObject.SendMessage("OnInteraction");
        }

        // Drop button
        if (Input.GetKeyDown(KeyCode.G))
        {
            InventorySystem.Instance.Drop();
        }

        // Flip button
        if (Input.GetKeyDown(KeyCode.F))
        {
            Hand.TurnHeldItem();
        }

        // Scroll wheel
        if (scrollWheelDelta != 0)
        {
            if (scrollWheelDelta > 0)
                InventorySystem.Instance.Next();
            else
                InventorySystem.Instance.Previous();
        }
    }

    // Get player SaveData
    public SaveData GetSaveData()
    {
        SaveData data = new SaveData();
        data.Gold = Gold;
        data.StrengthUpgradeLevel = StrengthUpgradeLevel;
        data.AgilityUpgradeLevel = AgilityUpgradeLevel;
        data.IntelligenceUpgradeLevel = IntelligenceUpgradeLevel;
        data.LuckUpgradeLevel = LuckUpgradeLevel;
        data.HealthUpgradeLevel = HealthUpgradeLevel;
        data.DefenseUpgradeLevel = DefenseUpgradeLevel;
        Debug.Log(data.ToString());
        return data;
    }

    // Set player SaveData
    public void SetSaveData(SaveData data)
    {
        Gold = data.Gold;
        StrengthUpgradeLevel = data.StrengthUpgradeLevel;
        AgilityUpgradeLevel = data.AgilityUpgradeLevel;
        IntelligenceUpgradeLevel = data.IntelligenceUpgradeLevel;
        LuckUpgradeLevel = data.LuckUpgradeLevel;
        HealthUpgradeLevel = data.HealthUpgradeLevel;
        DefenseUpgradeLevel = data.DefenseUpgradeLevel;
        Strength += StrengthUpgradeLevel;
        Agility += AgilityUpgradeLevel;
        Intelligence += IntelligenceUpgradeLevel;
        Luck += LuckUpgradeLevel;
        MaxHealth += HealthUpgradeLevel;
        Defense += DefenseUpgradeLevel;
        Health = MaxHealth;
    }
    private protected override void CalculateMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveDelta = new Vector2(x, y);
        moveDelta.Normalize();
        moveDelta *= MovementSpeed * Time.fixedDeltaTime;
    }

    private protected override void Move()
    {
        if (moveDelta != Vector3.zero)
        {
            // Checking for collision on X axis
            raycastHit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, new Vector2(moveDelta.x, 0),
                0.01f, LayerMask.GetMask("Actor", "Blocking"));

            if (raycastHit.collider == null)
            {
                // Applying movement on X axis
                transform.Translate(moveDelta.x, 0, 0);
            }

            // Checking for collision on Y axis
            raycastHit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, new Vector2(0, moveDelta.y),
                0.01f, LayerMask.GetMask("Actor", "Blocking"));

            if (raycastHit.collider == null)
            {
                // Applying movement on Y axis
                transform.Translate(0, moveDelta.y, 0);
            }
        }
    }

    private void FindInteractable()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Interactable");

        // Set interactionObject to first closest or null
        interactionObject = objects.ToList()
            .Where(o => Vector3.Distance(transform.position, o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)) <= InteractionRange)
            .OrderBy(o => Vector3.Distance(transform.position, o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)))
            .FirstOrDefault();
    }

    private void OnDrawGizmos()
    {
        //if (interactionObject == null) Gizmos.color = Color.yellow;
        //else Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, InteractionRange);
    }
}

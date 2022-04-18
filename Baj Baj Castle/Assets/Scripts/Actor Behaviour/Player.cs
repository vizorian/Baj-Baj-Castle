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

    private void Awake()
    {
        Instantiate();
    }

    private void Update()
    {
        ProcessInputs();
        LookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition), actorType);


        FindInteractable();
        if (interactionObject != null)
            interactionObject.SendMessage("OnCollide", _boxCollider);
    }

    private void FixedUpdate()
    {
        Move();
        _hand.UpdateCenterPosition(transform.position);
        _hand.LookTowards(Camera.main.ScreenToWorldPoint(Input.mousePosition));
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
    }

    private void Instantiate()
    {
        Health = 100;
        MaxHealth = 100;
        MovementSpeed = 0.5f;
        Defense = 0;
        Resistance = 0;

        InteractionRange = 0.15f;
        ReachRange = 0.1f;
        ViewRange = 0.5f;

        StrengthUpgradeLevel = 0;
        AgilityUpgradeLevel = 0;
        IntelligenceUpgradeLevel = 0;
        LuckUpgradeLevel = 0;

        Strength = 0;
        Agility = 0;
        Intelligence = 0;
        Luck = 0;
    }

    /// <summary>
    /// Processes the incoming inputs
    /// </summary>
    private void ProcessInputs()
    {
        // Getting inputs
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        float scrollWheelDelta = Input.GetAxisRaw("Mouse ScrollWheel");

        // Recalculating the movement direction
        moveDelta = new Vector2(x, y).normalized;

        // Left click
        if (Input.GetKey(KeyCode.Mouse0))
        {
            _hand.isFreezing = true;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _hand.isFreezing = false;
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
            TurnHeldItem();
        }

        // Reload button
        if (Input.GetKeyDown(KeyCode.R))
        {

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

        Gizmos.DrawWireSphere(transform.position, ReachRange);
    }
}

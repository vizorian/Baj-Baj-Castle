namespace Actor_Behaviour;

public class Player : Actor
{
    public int AgilityUpgradeLevel { get; set; }
    public int DefenseUpgradeLevel { get; set; }
    public int Gold { get; set; }
    public int HealthUpgradeLevel { get; set; }
    public int IntelligenceUpgradeLevel { get; set; }
    public int LuckUpgradeLevel { get; set; }
    public int StrengthUpgradeLevel { get; set; }

    [UsedImplicitly]
    private void Update()
    {
        ProcessInputs();
        CalculateMovement();
        Move();
        LookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition), ActorType);

        FindInteractable();
        if (InteractionObject != null)
            InteractionObject.SendMessage("OnCollide", BoxCollider);
    }

    private protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Hand.UpdateCenterPosition(transform.position);
        Hand.LookTowards(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    // Processes player inputs
    private void ProcessInputs()
    {
        var state = GlobalGameState.Escape;
        if (GameManager.Instance != null) state = GameManager.Instance.CurrentGlobalGameState;

        switch (state)
        {
            case GlobalGameState.MainMenu:
                return;
            case GlobalGameState.Escape:
                // Getting inputs
                var scrollWheelDelta = Input.GetAxisRaw("Mouse ScrollWheel");

                // Scroll wheel
                if (scrollWheelDelta != 0)
                {
                    if (scrollWheelDelta > 0)
                        InventorySystem.Instance.Next();
                    else
                        InventorySystem.Instance.Previous();
                }

                // Left click
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    if (Hand.HoldingItem)
                        if (Hand.HeldItemType == ItemType.Consumable)
                            Hand.UseHeldItem();
                    Hand.IsFreezingHand = true;
                }

                if (Input.GetKeyUp(KeyCode.Mouse0)) Hand.IsFreezingHand = false;

                // Interaction button
                if (Input.GetKeyDown(KeyCode.E) && InteractionObject != null)
                    InteractionObject.SendMessage("OnInteraction");

                // Drop button
                if (Input.GetKeyDown(KeyCode.G)) InventorySystem.Instance.Drop();

                // Flip button
                if (Input.GetKeyDown(KeyCode.F)) Hand.TurnHeldItem();
                break;
            case GlobalGameState.Tutorial:
                break;
            case GlobalGameState.Pause:
                break;
            case GlobalGameState.Loading:
                return;
            case GlobalGameState.Victory:
                return;
            case GlobalGameState.Defeat:
                return;
            case GlobalGameState.Reload:
                return;
        }
    }

    // Player death logic
    private protected override void Die()
    {
        GameManager.Instance.Defeat();
    }

    // Get player SaveData
    public SaveData GetSaveData()
    {
        var data = new SaveData
        {
            Gold = Gold,
            StrengthUpgradeLevel = StrengthUpgradeLevel,
            AgilityUpgradeLevel = AgilityUpgradeLevel,
            IntelligenceUpgradeLevel = IntelligenceUpgradeLevel,
            LuckUpgradeLevel = LuckUpgradeLevel,
            HealthUpgradeLevel = HealthUpgradeLevel,
            DefenseUpgradeLevel = DefenseUpgradeLevel
        };
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

        // Attribute integration
        MovementSpeed += Agility * 0.01f;
        MaxHealth += Strength;
        Resistance += (int)(Strength + Intelligence * 0.5f);
        Defense += (int)(Intelligence * 0.04f + Agility * 0.02f);
        Health = MaxHealth;
    }

    // Player movement calculation
    private protected override void CalculateMovement()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        MoveDelta = new Vector2(x, y);
        MoveDelta.Normalize();
        MoveDelta *= MovementSpeed;
        RigidBody.velocity = MoveDelta;
    }

    // Find closest interactable object to player
    private void FindInteractable()
    {
        var objects = GameObject.FindGameObjectsWithTag("Interactable");

        // Set interactionObject to first closest or null
        InteractionObject = objects.ToList()
            .Where(o => Vector3.Distance(transform.position,
                o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)) <= InteractionRange)
            .OrderBy(o =>
                Vector3.Distance(transform.position, o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)))
            .FirstOrDefault();
    }
}
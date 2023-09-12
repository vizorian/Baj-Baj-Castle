using System.Linq;
using Enums;
using Game_Logic;
using Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CreatureBehavior_Old
{
    public class Player : CreatureOld
    {
        // inputs

        private Keyboard _keyboard;
        private Mouse _mouse;

        // other shit

        public int AgilityUpgradeLevel { get; set; }
        public int DefenseUpgradeLevel { get; set; }
        public int Gold { get; set; }
        public int HealthUpgradeLevel { get; set; }
        public int IntelligenceUpgradeLevel { get; set; }
        public int LuckUpgradeLevel { get; set; }
        public int StrengthUpgradeLevel { get; set; }

        private protected override void Awake()
        {
            _keyboard = Keyboard.current;
            _mouse = Mouse.current;

            base.Awake();
        }

        private void Update()
        {
            // ProcessInputs();
            LookAt(Camera.main!.ScreenToWorldPoint(new Vector3(_mouse.position.x.value, _mouse.position.y.value)));

            FindInteractable();
            if (InteractionObject != null)
                InteractionObject.SendMessage("OnCollide", BoxCollider);
        }

        public void Move(InputAction.CallbackContext ctx)
        {
            Debug.Log("action: move");
            CalculateMovement();
            Move();
        }

        public void Look(InputAction.CallbackContext ctx)
        {
            
        }

        private protected override void FixedUpdate()
        {
            base.FixedUpdate();
        
            Hand.UpdateCenterPosition(transform.position);
            Hand.LookTowards(Camera.main.ScreenToWorldPoint(new Vector3(_mouse.position.x.value, _mouse.position.y.value)));
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
                    var scrollWheelDelta = _mouse.scroll.y.magnitude;

                    // Scroll wheel
                    if (scrollWheelDelta != 0)
                    {
                        if (scrollWheelDelta > 0)
                            InventorySystem.Instance.Next();
                        else
                            InventorySystem.Instance.Previous();
                    }

                    // Left click
                    if (_mouse.leftButton.wasPressedThisFrame)
                    {
                        if (Hand.HoldingItem)
                            if (Hand.HeldItemType == ItemType.Consumable)
                                Hand.UseHeldItem();
                        Hand.IsFreezingHand = true;
                    }

                    if (_mouse.leftButton.wasReleasedThisFrame) Hand.IsFreezingHand = false;

                    // Interaction button
                    if (_keyboard.eKey.wasPressedThisFrame && InteractionObject != null)
                        InteractionObject.SendMessage("OnInteraction");

                    // Drop button
                    if (_keyboard.gKey.wasPressedThisFrame) InventorySystem.Instance.Drop();

                    // Flip button
                    if (_keyboard.fKey.wasPressedThisFrame) Hand.TurnHeldItem();
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
            var x = _keyboard.dKey.magnitude - _keyboard.aKey.magnitude;
            var y = _keyboard.wKey.magnitude - _keyboard.sKey.magnitude;

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
                    Vector3.Distance(transform.position,
                        o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)))
                .FirstOrDefault();
        }
    }
}
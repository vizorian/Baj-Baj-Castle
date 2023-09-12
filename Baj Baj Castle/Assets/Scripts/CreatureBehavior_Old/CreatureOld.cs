using System.Collections.Generic;
using Combat;
using Enums;
using JetBrains.Annotations;
using UI;
using UnityEngine;

namespace CreatureBehavior_Old
{
    public class CreatureOld : MonoBehaviour
    {
        // Attributes
        public float MaxHealth;
        public float Health;
        public int Defense { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public int Luck { get; set; }
        public int Resistance { get; set; }

        // Graphics
        public Sprite SideSprite;
        public Sprite FrontSprite;
        public Sprite BackSprite;

        // Metadata
        public ActorType ActorType { get; }
    
        // Environmental
        public Hand Hand;
        private GameObject HandPrefab { get; set; }
        public float InteractionRange;
        public float MovementSpeed;
        public float ReachRange;
        public bool IsActive { get; set; }
        private readonly List<Collider2D> _hits = new();
        private protected BoxCollider2D BoxCollider;
        private ContactFilter2D _contactFilter;

        private protected GameObject InteractionObject;
        private Vector3 _knockbackDirection;
        private protected Vector3 MoveDelta;
        private protected Rigidbody2D RigidBody;
        private SpriteRenderer _spriteRenderer;
        private protected GameObject Target;

        private protected virtual void Awake()
        {
            // Atribute usage 
            MovementSpeed += Agility * 0.01f;
            MaxHealth += Strength;
            Resistance += (int)(Strength + Intelligence * 0.5f);
            Defense += (int)(Intelligence * 0.2f + Agility * 0.01f);
            Health = MaxHealth;


            RigidBody = GetComponent<Rigidbody2D>();
            BoxCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _contactFilter = new ContactFilter2D();

            if (HandPrefab == null) return;
            Hand = Instantiate(HandPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform)
                .GetComponent<Hand>();
            Hand.Init(ReachRange);
            Hand.UpdateCenterPosition(transform.position);
        }

        private protected virtual void FixedUpdate()
        {
            // Handling collisions
            BoxCollider.OverlapCollider(_contactFilter, _hits);
            for (var i = 0; i < _hits.Count; i++)
            {
                OnCollide(_hits[i]);
                _hits[i] = null;
            }
        }

        private protected virtual void OnCollide(Collider2D otherCollider)
        {
        }

        // Used to receive and process damage
        // Called by weapons on collision
        [UsedImplicitly]
        private protected virtual void TakeDamage(DamageData damageData)
        {
            var knockback = damageData.Knockback;
            var damage = damageData.Amount;

            // Adjust damage based on if the weapon is flipped or not
            if (damageData.Source.Hand != null)
            {
                if (damageData.Type == DamageType.Piercing && damageData.Source.Hand.IsItemTurned)
                    damageData.Type = DamageType.Slashing;
                else if (damageData.Type == DamageType.Slashing && damageData.Source.Hand.IsItemTurned)
                    damageData.Type = DamageType.Piercing;
            }

            // Damage types
            if (damageData.Type == DamageType.Piercing)
                damage -= Defense / 4;
            else if (damageData.Type == DamageType.Bludgeoning)
                damage -= Defense / 2;
            else if (damageData.Type == DamageType.Slashing) damage -= Defense;

            if (damage < 1)
                damage = 1;

            if (damageData.IsCritical)
            {
                damage *= 2;
                knockback *= 2;
                FloatingText.Create(damage.ToString(), Color.yellow, transform.position, 1.2f, 0.5f, 0.2f);
            }
            else
            {
                FloatingText.Create(damage.ToString(), Color.red, transform.position, 1f, 0.5f, 0.2f);
            }

            CalculateKnockback(damageData.Source.transform.position, knockback);

            // Apply damage
            Health -= damage;
            if (Health <= 0) Die();
        }

        // Used to receive and process healing
        public virtual void Heal(float amount)
        {
            if (Health + amount > MaxHealth)
                Health = MaxHealth;
            else
                Health += amount;

            FloatingText.Create(amount.ToString(), Color.green, transform.position, 1f, 0.5f, 0.2f);
        }

        // Used to calculate knocback direction and strength
        private void CalculateKnockback(Vector3 from, float distance)
        {
            _knockbackDirection = transform.position - from;
            _knockbackDirection.Normalize();
            _knockbackDirection *= distance;
        }

        // Actor death logic
        private protected virtual void Die()
        {
            Destroy(gameObject);
        }

        // Actor movement logic
        private protected virtual void Move()
        {
            // Knockback processing
            if (Resistance >= 100)
            {
                _knockbackDirection = Vector3.zero;
            }
            else
            {
                var realResistance = Resistance / 100f;
                if (realResistance == 0) realResistance = 0.01f;
                _knockbackDirection = Vector3.Lerp(_knockbackDirection, Vector3.zero, realResistance);
            }

            MoveDelta += _knockbackDirection;

            // Final movement
            RigidBody.velocity = MoveDelta;
        }

        // Movement calculation
        private protected virtual void CalculateMovement()
        {
            MoveDelta = Target.transform.position - transform.position;
            MoveDelta.Normalize();
            MoveDelta *= MovementSpeed;
        }

        // Actor looking logic
        private protected void LookAt(Vector3 position)
        {
            // Calculating position difference between the target and actor
            var posDif = position - transform.position;

            // Calculating the angle of the target relative to the actor
            var z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
            if (z < 0) z = 180 + (180 - Mathf.Abs(z));

            // Updating sprite to look at target
            if (z is >= 45 and < 135)
            {
                _spriteRenderer.sprite = BackSprite;
            }
            else if (z is >= 135 and < 225)
            {
                _spriteRenderer.sprite = SideSprite;
                _spriteRenderer.flipX = true;
            }
            else if (z is >= 225 and < 315)
            {
                _spriteRenderer.sprite = FrontSprite;
            }
            else
            {
                _spriteRenderer.sprite = SideSprite;
                _spriteRenderer.flipX = false;
            }
        }
    }
}
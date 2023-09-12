using Combat;
using Enums;
using JetBrains.Annotations;
using UnityEngine;

namespace CreatureBehavior_Old
{
    [UsedImplicitly]
    public class Rusher : CreatureOld
    {
        public float AttackSpeed;

        public float CriticalChance;

        // Attributes
        public int Damage;
        public DamageType DamageType;
        public float Knockback;
        public float PauseTime;
        private float _attackTimer;
        private float _pauseTimer;

        private protected override void Awake()
        {
            base.Awake();
            PauseTime = AttackSpeed;
            CriticalChance += Luck * 0.5f;
            Knockback += Strength * 0.1f;
            Damage += (int)(Strength * 0.2f);
        }

        [UsedImplicitly]
        private void Update()
        {
            if (IsActive)
            {
                if (Target == null)
                {
                    FindAndSetTarget();
                }
                else
                {
                    CalculateMovement();
                    if (_pauseTimer <= 0) Move();
                }

                // Attack cooldown
                if (_attackTimer > 0) _attackTimer -= Time.deltaTime;

                // Pause cooldown
                if (_pauseTimer > 0) _pauseTimer -= Time.deltaTime;
            }
        }

        private protected override void FixedUpdate()
        {
            if (IsActive)
            {
                base.FixedUpdate();

                if (Target != null)
                {
                    LookAt(Target.transform.position);
                    if (Hand != null)
                    {
                        Hand.UpdateCenterPosition(transform.position);
                        if (Target != null) Hand.LookTowards(Target.transform.position);
                    }
                }
            }
        }

        // Handle collisions
        // Does damage to player and objects
        private protected override void OnCollide(Collider2D otherCollider)
        {
            if (otherCollider.gameObject.tag == "Player"
                || otherCollider.gameObject.tag == "Object")
            {
                // Attack cooldown
                if (_attackTimer > 0) return;

                // Pause cooldown
                if (otherCollider.gameObject.tag == "Player") _pauseTimer = PauseTime;

                var damageData = new DamageData(Damage, DamageType, Knockback, this)
                {
                    IsCritical = Random.Range(0, 101) <= CriticalChance
                };
                otherCollider.gameObject.SendMessage("TakeDamage", damageData);

                _attackTimer = AttackSpeed;
            }
        }

        // Finds and sets a target
        private void FindAndSetTarget()
        {
            Target = null;
            var player = GameObject.FindGameObjectWithTag("Player");
            Target = player;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rusher : Actor
{
    // Attributes
    public int Damage;
    public DamageType DamageType;
    public float CriticalChance;
    public float Knockback;
    public float AttackSpeed;
    public float CooldownTimer = 0;

    private void Update()
    {
        if (IsActive)
        {
            if (target == null)
            {
                FindAndSetTarget();
            }
            else
            {
                Move();
                CalculateMovement();
            }

            // Attack cooldown
            if (CooldownTimer > 0)
            {
                CooldownTimer -= Time.deltaTime;
            }
        }
    }

    private protected override void FixedUpdate()
    {
        if (IsActive)
        {
            base.FixedUpdate();

            if (target != null)
            {
                LookAt(target.transform.position, ActorType);
                if (Hand != null)
                {
                    Hand.UpdateCenterPosition(transform.position);
                    if (target != null)
                    {
                        Hand.LookTowards(target.transform.position);

                    }
                }
            }
        }
    }

    // handle collisions
    private protected override void OnCollide(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            // Attack cooldown
            if (CooldownTimer > 0)
            {
                return;
            }

            var damageData = new DamageData(Damage, DamageType, Knockback, this);
            damageData.IsCritical = Random.Range(0, 101) <= CriticalChance;
            collider.gameObject.SendMessage("TakeDamage", damageData);

            CooldownTimer = AttackSpeed;
        }
    }

    private void FindAndSetTarget()
    {
        target = null;
        var player = GameObject.FindGameObjectWithTag("Player");
        target = player;
    }
}

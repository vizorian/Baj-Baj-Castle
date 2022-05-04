using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class Destructable : MonoBehaviour
{
    public float Health;

    [UsedImplicitly]
    private protected virtual void TakeDamage(DamageData damageData)
    {
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
            damage /= 4;
        else if (damageData.Type == DamageType.Slashing) damage /= 2;

        if (damage < 1)
            damage = 1;


        Health -= damage;
        FloatingText.Create(damage.ToString(), Color.grey, transform.position, 1f, 0.5f, 0.2f);
        if (Health <= 0) Die();
    }

    private protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
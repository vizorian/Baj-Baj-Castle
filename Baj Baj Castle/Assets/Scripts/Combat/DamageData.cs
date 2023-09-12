using CreatureBehavior;
using CreatureBehavior_Old;
using Enums;

namespace Combat
{
    public class DamageData
    {
        public float Amount;
        public bool IsCritical = false;
        public float Knockback;
        public CreatureOld Source;
        public DamageType Type;

        public DamageData(float amount, DamageType type, float knockback, CreatureOld source)
        {
            Amount = amount;
            Type = type;
            Knockback = knockback;
            Source = source;
        }
    }
}
using System;
using Enums;

namespace Combat
{
    [Serializable]
    public class ItemProperties
    {
        public float Damage;
        public DamageType DamageType;
        public float CriticalChance;
        public float Speed;
        public float Cooldown;
        public float Knockback;
        public float Range;
    }
}
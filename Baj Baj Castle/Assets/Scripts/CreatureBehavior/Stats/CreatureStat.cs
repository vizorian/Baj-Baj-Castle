using System.Collections.Generic;

namespace CreatureBehavior.Stats
{
    public class CreatureStat
    {
        public int BaseValue { get; set; }

        private readonly List<StatModifier> _statModifiers;

        public CreatureStat(int baseValue)
        {
            BaseValue = baseValue;
            _statModifiers = new List<StatModifier>();
        }
    }
}
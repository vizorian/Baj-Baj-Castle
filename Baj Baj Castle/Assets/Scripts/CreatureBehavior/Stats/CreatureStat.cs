using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CreatureBehavior.Stats
{
    public class CreatureStat
    {
        public int BaseValue { get; }
        private readonly List<StatModifier> _statModifiers;

        public int Value
        {
            get
            {
                if (_isDirty)
                {
                    _value = CalculateFinalValue();
                    _isDirty = false;
                }

                return _value;
            }
        }

        private int _value;
        private bool _isDirty = true;

        public CreatureStat(int baseValue)
        {
            BaseValue = baseValue;
            _value = CalculateFinalValue();
            _statModifiers = new List<StatModifier>();
        }

        public void AddModifier(StatModifier modifier)
        {
            _statModifiers.Add(modifier);
            _isDirty = true;
        }

        public void RemoveModifier(StatModifier modifier)
        {
            _statModifiers.Remove(modifier);
            _isDirty = true;
        }

        // TODO: modifier calculation logic
        private int CalculateFinalValue()
        {
            float finalValue = BaseValue;

            foreach (var statModifier in _statModifiers)
            {
                switch (statModifier.Type)
                {
                    case StatModifierType.Flat:
                        break;
                    case StatModifierType.Percentage:
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
                finalValue += statModifier.Value;
            }

            return (int)Math.Round(finalValue);
        }
    }
}
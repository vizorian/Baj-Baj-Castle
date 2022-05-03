using System;
using JetBrains.Annotations;

[Serializable]
public enum DamageType
{
    [UsedImplicitly] None,
    Slashing,
    Bludgeoning,
    Piercing
}
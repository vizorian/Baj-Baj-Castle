using System;
using JetBrains.Annotations;

namespace Enums;

[Serializable]
public enum DamageType
{
    [UsedImplicitly] None,
    Slashing,
    Bludgeoning,
    Piercing
}
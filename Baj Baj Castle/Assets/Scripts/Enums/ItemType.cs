using JetBrains.Annotations;

namespace Enums;

public enum ItemType
{
    None,
    Consumable,
    Weapon,
    [UsedImplicitly] Misc
}
using System;

[Serializable]
public class SaveData
{
    public int Gold;
    public int StrengthUpgradeLevel;
    public int AgilityUpgradeLevel;
    public int IntelligenceUpgradeLevel;
    public int LuckUpgradeLevel;

    public SaveData()
    {
        Gold = 0;
        StrengthUpgradeLevel = 0;
        AgilityUpgradeLevel = 0;
        IntelligenceUpgradeLevel = 0;
        LuckUpgradeLevel = 0;
    }

    public override string ToString()
    {
        return string.Format("Gold: {0}, Strength: {1}, Agility: {2}, Intelligence: {3}, Luck: {4}", Gold, StrengthUpgradeLevel, AgilityUpgradeLevel, IntelligenceUpgradeLevel, LuckUpgradeLevel);
    }
}
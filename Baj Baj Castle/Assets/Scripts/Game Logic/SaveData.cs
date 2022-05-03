using System;

[Serializable]
public class SaveData
{
    public int AgilityUpgradeLevel;
    public int DefenseUpgradeLevel;
    public int Gold;
    public int HealthUpgradeLevel;
    public int IntelligenceUpgradeLevel;
    public int LuckUpgradeLevel;
    public int StrengthUpgradeLevel;

    public SaveData()
    {
        Gold = 0;
        StrengthUpgradeLevel = 0;
        AgilityUpgradeLevel = 0;
        IntelligenceUpgradeLevel = 0;
        LuckUpgradeLevel = 0;
        HealthUpgradeLevel = 0;
        DefenseUpgradeLevel = 0;
    }

    public override string ToString()
    {
        return string.Format(
            "Gold: {0}, Strength: {1}, Agility: {2}, Intelligence: {3}, Luck: {4}, Health: {5}, Defense {6}",
            Gold,
            StrengthUpgradeLevel,
            AgilityUpgradeLevel,
            IntelligenceUpgradeLevel,
            LuckUpgradeLevel,
            HealthUpgradeLevel,
            DefenseUpgradeLevel);
    }

    public int GetStat(string stat)
    {
        switch (stat)
        {
            case "Strength":
                return StrengthUpgradeLevel;
            case "Agility":
                return AgilityUpgradeLevel;
            case "Intelligence":
                return IntelligenceUpgradeLevel;
            case "Luck":
                return LuckUpgradeLevel;
            case "Health":
                return HealthUpgradeLevel;
            case "Defense":
                return DefenseUpgradeLevel;
            default:
                return 0;
        }
    }
}
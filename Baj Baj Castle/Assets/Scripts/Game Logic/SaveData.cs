using System;

[Serializable]
public class SaveData
{
    public bool IsNewGame;
    public int Gold;
    public int StrengthUpgradeLevel;
    public int AgilityUpgradeLevel;
    public int IntelligenceUpgradeLevel;
    public int LuckUpgradeLevel;
    public int HealthUpgradeLevel;
    public int DefenseUpgradeLevel;

    public SaveData()
    {
        IsNewGame = true;
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
        return string.Format("NewGame? {0} Gold: {1}, Strength: {2}, Agility: {3}, Intelligence: {4}, Luck: {5}, Health: {6}, Defense {7}",
                             IsNewGame,
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
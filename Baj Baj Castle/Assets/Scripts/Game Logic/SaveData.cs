namespace Game_Logic;

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
        return $"Gold: {Gold}, Strength: {StrengthUpgradeLevel}, Agility: {AgilityUpgradeLevel}, Intelligence: {IntelligenceUpgradeLevel}, Luck: {LuckUpgradeLevel}, Health: {HealthUpgradeLevel}, Defense {DefenseUpgradeLevel}";
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
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

    public SaveData()
    {
        IsNewGame = true;
        Gold = 0;
        StrengthUpgradeLevel = 0;
        AgilityUpgradeLevel = 0;
        IntelligenceUpgradeLevel = 0;
        LuckUpgradeLevel = 0;
    }

    public override string ToString()
    {
        return string.Format("NewGame? {0} Gold: {1}, Strength: {2}, Agility: {3}, Intelligence: {4}, Luck: {5}", IsNewGame, Gold, StrengthUpgradeLevel, AgilityUpgradeLevel, IntelligenceUpgradeLevel, LuckUpgradeLevel);
    }
}
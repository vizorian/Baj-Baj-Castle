using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class SaveDataTests
    {
        [TestMethod]
        public void SaveDataTest()
        {
            var saveData = new SaveData();

            Assert.AreEqual(0, saveData.Gold);
            Assert.AreEqual(0, saveData.StrengthUpgradeLevel);
            Assert.AreEqual(0, saveData.AgilityUpgradeLevel);
            Assert.AreEqual(0, saveData.IntelligenceUpgradeLevel);
            Assert.AreEqual(0, saveData.LuckUpgradeLevel);
            Assert.AreEqual(0, saveData.HealthUpgradeLevel);
            Assert.AreEqual(0, saveData.DefenseUpgradeLevel);
        }

        [TestMethod]
        public void ToStringTest()
        {
            var gold = 10;
            var strengthLevel = 1;
            var agilityLevel = 2;
            var intelligenceLevel = 3;
            var luckLevel = 4;
            var defenseLevel = 5;
            var healthLevel = 6;

            var saveData = new SaveData()
            {
                Gold = gold,
                StrengthUpgradeLevel = strengthLevel,
                AgilityUpgradeLevel = agilityLevel,
                IntelligenceUpgradeLevel = intelligenceLevel,
                LuckUpgradeLevel = luckLevel,
                DefenseUpgradeLevel = defenseLevel,
                HealthUpgradeLevel = healthLevel
            };

            var newString = saveData.ToString();

            Assert.IsTrue(newString.Contains(gold.ToString()));
            Assert.IsTrue(newString.Contains(strengthLevel.ToString()));
            Assert.IsTrue(newString.Contains(agilityLevel.ToString()));
            Assert.IsTrue(newString.Contains(intelligenceLevel.ToString()));
            Assert.IsTrue(newString.Contains(luckLevel.ToString()));
            Assert.IsTrue(newString.Contains(defenseLevel.ToString()));
            Assert.IsTrue(newString.Contains(healthLevel.ToString()));
        }

        [TestMethod]
        public void GetStatTest()
        {
            var strengthLevel = 1;
            var agilityLevel = 2;
            var intelligenceLevel = 3;
            var luckLevel = 4;
            var defenseLevel = 5;
            var healthLevel = 6;

            var saveData = new SaveData()
            {
                Gold = 0,
                StrengthUpgradeLevel = strengthLevel,
                AgilityUpgradeLevel = agilityLevel,
                IntelligenceUpgradeLevel = intelligenceLevel,
                LuckUpgradeLevel = luckLevel,
                DefenseUpgradeLevel = defenseLevel,
                HealthUpgradeLevel = healthLevel
            };

            Assert.AreEqual(strengthLevel, saveData.GetStat("Strength"));
            Assert.AreEqual(agilityLevel, saveData.GetStat("Agility"));
            Assert.AreEqual(intelligenceLevel, saveData.GetStat("Intelligence"));
            Assert.AreEqual(luckLevel, saveData.GetStat("Luck"));
            Assert.AreEqual(defenseLevel, saveData.GetStat("Defense"));
            Assert.AreEqual(healthLevel, saveData.GetStat("Health"));
        }
    }
}
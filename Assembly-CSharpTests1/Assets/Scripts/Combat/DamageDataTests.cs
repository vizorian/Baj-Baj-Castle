using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DamageDataTests
    {
        [TestMethod]
        public void DamageDataTest()
        {
            var damageData = new DamageData(1, DamageType.Piercing, 1f, null);
            Assert.IsNotNull(damageData);
            Assert.AreEqual(1, damageData.Amount);
            Assert.AreEqual(DamageType.Piercing, damageData.Type);
            Assert.IsTrue(damageData.Knockback > 0f);
        }
    }
}
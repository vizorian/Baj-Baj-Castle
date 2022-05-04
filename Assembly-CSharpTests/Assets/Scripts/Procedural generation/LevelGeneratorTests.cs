using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class LevelGeneratorTests
    {
        [TestMethod]
        public void RoundNumberTest()
        {
            var roundTo = 5;
            var x = 4;
            Assert.AreEqual(LevelGenerator.RoundNumber(x, roundTo), 5);

            x = 5;
            Assert.AreEqual(LevelGenerator.RoundNumber(x, roundTo), 5);

            x = 6;
            Assert.AreEqual(LevelGenerator.RoundNumber(x, roundTo), 10);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
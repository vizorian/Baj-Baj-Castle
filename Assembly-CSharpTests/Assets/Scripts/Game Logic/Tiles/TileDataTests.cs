using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TileDataTests
    {
        [TestMethod]
        public void TileDataTest()
        {
            var x = 1;
            var y = 2;
            var tileType = TileType.Floor;

            var tileData = new TileData(x, y, tileType);
            Assert.IsNotNull(tileData);
            Assert.AreEqual(x, tileData.X);
            Assert.AreEqual(y, tileData.Y);
            Assert.AreEqual(tileType, tileData.Type);
        }

        [TestMethod]
        public void ToStringTest()
        {
            var x = 1;
            var y = 2;
            var tileType = TileType.Floor;

            var tileData = new TileData(x, y, tileType);

            var newString = tileData.ToString();
            Assert.IsTrue(newString.Contains(x.ToString()));
            Assert.IsTrue(newString.Contains(y.ToString()));
            Assert.IsTrue(newString.Contains(tileType.ToString()));
        }

        [TestMethod]
        public void EqualsTest()
        {
            var x = 1;
            var y = 2;
            var tileType = TileType.Floor;

            var tileData = new TileData(x, y, tileType);

            var otherX = 2;
            var otherY = 3;
            var otherTileType = TileType.Floor;

            var otherTileData = new TileData(x, y, tileType);
            Assert.AreEqual(tileData, otherTileData);
            otherTileData = new TileData(otherX, otherY, otherTileType);
            Assert.AreNotEqual(tileData, otherTileData);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var x = 1;
            var y = 2;
            var tileType = TileType.Floor;

            var tileData = new TileData(x, y, tileType);

            var otherX = 2;
            var otherY = 3;
            var otherTileType = TileType.Floor;

            var otherTileData = new TileData(otherX, otherY, otherTileType);

            var hash = tileData.GetHashCode();
            var otherHash = otherTileData.GetHashCode();

            Assert.AreNotEqual(hash, otherHash);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class PointTests
    {
        [TestMethod]
        public void PointTest()
        {
            var x = 1;
            var y = 2;
            var point = new Point(x, y);

            Assert.AreEqual(x, point.X);
            Assert.AreEqual(y, point.Y);
        }

        [TestMethod]
        public void DistanceToTest()
        {
            var x1 = 1;
            var x2 = 1;

            var y1 = 2;
            var y2 = 4;

            var point = new Point(x1, y1);
            var otherPoint = new Point(x2, y2);

            Assert.AreEqual(point.DistanceTo(otherPoint), y2 - y1);
        }

        [TestMethod]
        public void EqualsTest()
        {
            var point = new Point(1, 2);
            var otherPoint = new Point(1, 2);

            Assert.AreEqual(point, otherPoint);

            otherPoint = new Point(3, 4);
            Assert.AreNotEqual(point, otherPoint);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var point = new Point(1, 2);
            var otherPoint = new Point(1, 2);

            Assert.AreEqual(point.GetHashCode(), otherPoint.GetHashCode());

            otherPoint = new Point(1, 4);
            Assert.AreNotEqual(point.GetHashCode(), otherPoint.GetHashCode());
        }

        [TestMethod]
        public void ToStringTest()
        {
            var x = 1;
            var y = 2;

            var point = new Point(x, y);
            var newString = point.ToString();

            Assert.IsTrue(newString.Length >= 5);
            Assert.IsTrue(newString.Contains(x.ToString()));
            Assert.IsTrue(newString.Contains(y.ToString()));
        }
    }
}
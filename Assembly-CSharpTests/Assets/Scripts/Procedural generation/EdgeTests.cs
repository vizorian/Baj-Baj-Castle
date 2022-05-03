using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class EdgeTests
    {
        [TestMethod]
        public void EdgeTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);

            var edge = new Edge(p1, p2);
            Assert.AreEqual(p1, edge.P1);
            Assert.AreEqual(p2, edge.P2);
        }

        [TestMethod]
        public void EqualsTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);

            var edge = new Edge(p1, p2);

            var otherP1 = new Point(4, 5);
            var otherP2 = new Point(6, 7);

            var otherEdge = new Edge(p1, p2);

            Assert.AreEqual(edge, otherEdge);

            otherEdge = new Edge(otherP1, otherP2);
            Assert.AreNotEqual(edge, otherEdge);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);

            var edge = new Edge(p1, p2);

            var otherP1 = new Point(4, 5);
            var otherP2 = new Point(6, 7);

            var otherEdge = new Edge(otherP1, otherP2);

            var hash = edge.GetHashCode();
            var otherHash = otherEdge.GetHashCode();

            Assert.AreNotEqual(hash, otherHash);
        }
    }
}
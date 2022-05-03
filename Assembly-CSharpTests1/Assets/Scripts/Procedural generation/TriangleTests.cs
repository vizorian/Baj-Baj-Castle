using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TriangleTests
    {
        [TestMethod]
        public void TriangleTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);
            var p3 = new Point(3, 4);

            var triangle = new Triangle(p1, p2, p3);
            Assert.AreEqual(p1, triangle.Vertices[0]);
            Assert.AreEqual(p2, triangle.Vertices[1]);
            Assert.AreEqual(p3, triangle.Vertices[2]);
            Assert.IsTrue(triangle.Circumcenter != null);
        }

        [TestMethod]
        public void ContainsEdgeTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);
            var p3 = new Point(3, 4);

            var triangle = new Triangle(p1, p2, p3);

            var edge1 = new Edge(p1, p2);
            var edge2 = new Edge(p2, p3);
            var edge3 = new Edge(p3, p1);

            Assert.IsTrue(triangle.ContainsEdge(edge1));
            Assert.IsTrue(triangle.ContainsEdge(edge2));
            Assert.IsTrue(triangle.ContainsEdge(edge3));
        }

        [TestMethod]
        public void IsWithinCircumcircleTest()
        {
            var p1 = new Point(1, 6);
            var p2 = new Point(3, 6);
            var p3 = new Point(6, 1);

            var p = new Point(3, 3);

            var triangle = new Triangle(p1, p2, p3);

            Assert.IsTrue(triangle.IsWithinCircumcircle(p));
        }

        [TestMethod]
        public void ToStringTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);
            var p3 = new Point(3, 4);

            var triangle = new Triangle(p1, p2, p3);

            var newString = triangle.ToString();

            Assert.IsTrue(newString.Length > 6 + triangle.Vertices.Length * 2);
            Assert.IsTrue(newString.Contains(triangle.Vertices[0].X.ToString()));
            Assert.IsTrue(newString.Contains(triangle.Vertices[0].Y.ToString()));
            Assert.IsTrue(newString.Contains(triangle.Vertices[1].X.ToString()));
            Assert.IsTrue(newString.Contains(triangle.Vertices[1].Y.ToString()));
            Assert.IsTrue(newString.Contains(triangle.Vertices[2].X.ToString()));
            Assert.IsTrue(newString.Contains(triangle.Vertices[2].Y.ToString()));
        }

        [TestMethod]
        public void EqualsTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);
            var p3 = new Point(3, 4);

            var triangle = new Triangle(p1, p2, p3);

            var otherP1 = new Point(5, 6);
            var otherP2 = new Point(7, 8);
            var otherP3 = new Point(9, 10);

            var otherTriangle = new Triangle(p1, p2, p3);

            Assert.AreEqual(triangle, otherTriangle);

            otherTriangle = new Triangle(otherP1, otherP2, otherP3);
            Assert.AreNotEqual(triangle, otherTriangle);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(2, 3);
            var p3 = new Point(3, 4);

            var triangle = new Triangle(p1, p2, p3);

            var otherP1 = new Point(5, 6);
            var otherP2 = new Point(7, 8);
            var otherP3 = new Point(9, 10);

            var otherTriangle = new Triangle(otherP1, otherP2, otherP3);

            var hash = triangle.GetHashCode();
            var otherHash = otherTriangle.GetHashCode();

            Assert.AreNotEqual(hash, otherHash);
        }
    }
}
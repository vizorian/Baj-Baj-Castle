using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class DelaunayTriangulatorTests
    {
        [TestMethod]
        public void BowyerWatsonTest()
        {
            var points = new HashSet<Point>();
            points.Add(new Point(0, 0));
            points.Add(new Point(0, 4));
            points.Add(new Point(4, 0));
            points.Add(new Point(4, 4));
            points.Add(new Point(2, 2));

            var triangulator = new DelaunayTriangulator();
            var triangulation = triangulator.BowyerWatson(points);

            Assert.IsNotNull(triangulation);
            Assert.IsTrue(triangulation.Count == 4);
        }
    }
}
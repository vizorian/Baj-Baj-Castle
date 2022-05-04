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
            var points = new HashSet<Point>
            {
                new Point(0, 0),
                new Point(0, 4),
                new Point(4, 0),
                new Point(4, 4),
                new Point(2, 2)
            };

            var triangulator = new DelaunayTriangulator();
            var triangulation = triangulator.BowyerWatson(points);

            Assert.IsNotNull(triangulation);
            Assert.IsTrue(triangulation.Count == 4);
        }
    }
}
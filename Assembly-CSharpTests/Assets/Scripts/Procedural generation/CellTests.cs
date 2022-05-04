using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

namespace Tests
{
    [TestClass]
    public class CellTests
    {
        [TestMethod]
        public void CellTest()
        {
            var position = new Vector2(0, 0);
            var width = 1;
            var height = 1;

            var cell = new Cell(position, width, height);

            Assert.AreEqual(position, cell.Position);
            Assert.AreEqual(width, cell.Width);
            Assert.AreEqual(height, cell.Height);
        }

        //[TestMethod]
        //public void CreatePhysicsCellObjectTest()
        //{
        //    var position = new Vector2(0, 0);
        //    var width = 1;
        //    var height = 1;

        //    var cell = new Cell(position, width, height);

        //    Assert.IsNull(cell.PhysicsCell);
        //cell.CreatePhysicsCellObject(1, null);
        //Assert.IsNotNull(cell.PhysicsCell);
        //}

        //[TestMethod]
        //public void CreateSimulationCellObjectTest()
        //{
        //    var position = new Vector2(0, 0);
        //    var width = 1;
        //    var height = 1;

        //    var cell = new Cell(position, width, height);

        //    Assert.IsNull(cell.SimulationCell);
        //    cell.CreateSimulationCellObject(1, null);
        //    Assert.IsNotNull(cell.SimulationCell);
        //}

        [TestMethod]
        public void IsPointInsideTest()
        {
            var position = new Vector2(0, 0);
            var width = 10;
            var height = 10;

            var cell = new Cell(position, width, height);

            var point = new Point(0, 0);
            var outsidePoint = new Point(20, 20);

            Assert.IsTrue(cell.IsPointInside(point));
            Assert.IsFalse(cell.IsPointInside(outsidePoint));
        }

        //[TestMethod]
        //public void OverlapsTest()
        //{
        //    var position = new Vector2(0, 0);
        //    var width = 2;
        //    var height = 2;

        //    var cell = new Cell(position, width, height);

        //    var otherPosition = new Vector2(2, 0);
        //    var nearbyCell = new Cell(otherPosition, width, height);

        //    cell.CreateSimulationCellObject(1, null);
        //    nearbyCell.CreateSimulationCellObject(1, null);

        //    Assert.IsFalse(cell.Overlaps(nearbyCell.SimulationCell, 0.001f));

        //    var overlappingCell = new Cell(position, width, height);
        //    overlappingCell.CreateSimulationCellObject(1, null);
        //    Assert.IsTrue(cell.Overlaps(overlappingCell.SimulationCell, 0.001f));
        //}

        //[TestMethod]
        //public void CreateDisplayCellObjectTest()
        //{
        //    var position = new Vector2(0, 0);
        //    var width = 1;
        //    var height = 1;

        //    var cell = new Cell(position, width, height);

        //    Assert.IsNull(cell.DisplayCell);
        //    cell.CreateDisplayCellObject(null, Color.white);
        //    Assert.IsNotNull(cell.DisplayCell);
        //}

        //[TestMethod]
        //public void IsPartOfTest()
        //{
        //    var position = new Vector2(1, 2);
        //    var width = 2;
        //    var height = 2;

        //    var cell = new Cell(position, width, height);
        //    cell.CreateSimulationCellObject(1, null);

        //    var p1 = new Point(1, 2);
        //    var p2 = new Point(2, 3);
        //    var p3 = new Point(3, 4);

        //    var triangle = new Triangle(p1, p2, p3);
        //    var triangleHashSet = new HashSet<Triangle>();
        //    triangleHashSet.Add(triangle);

        //    Assert.IsTrue(cell.IsPartOf(triangleHashSet));
        //}
    }
}
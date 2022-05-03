using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DisjointSetTests
    {
        [TestMethod]
        public void DisjointSetTest()
        {
            var disjointSet = new DisjointSet(3);
            Assert.IsNotNull(disjointSet);
        }

        [TestMethod]
        public void MakeSetTest()
        {
            var disjointSet = new DisjointSet(3);
            disjointSet.MakeSet(1);
            disjointSet.MakeSet(2);
            disjointSet.MakeSet(3);

            var result = disjointSet.Find(1);
            Assert.IsNotNull(result);

            result = disjointSet.Find(2);
            Assert.IsNotNull(result);

            result = disjointSet.Find(3);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindTest()
        {
            var disjointSet = new DisjointSet(3);
            disjointSet.MakeSet(1);
            disjointSet.MakeSet(2);
            disjointSet.MakeSet(3);

            var first = disjointSet.Find(1);
            Assert.IsNotNull(first);
        }

        [TestMethod]
        public void UnionTest()
        {
            var disjointSet = new DisjointSet(2);
            disjointSet.MakeSet(1);
            disjointSet.MakeSet(2);

            var result = disjointSet.Find(1);
            var otherResult = disjointSet.Find(2);
            Assert.AreNotEqual(result, otherResult);

            disjointSet.Union(1, 2);
            result = disjointSet.Find(1);
            otherResult = disjointSet.Find(2);
            Assert.AreEqual(result, otherResult);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class InventoryItemTests
    {
        [TestMethod]
        public void InventoryItemTest()
        {
            var item = new InventoryItem(null);
            Assert.IsTrue(item.StackSize == 1);
        }

        [TestMethod]
        public void AddToStackTest()
        {
            var item = new InventoryItem(null);

            Assert.IsTrue(item.StackSize == 1);
            item.AddToStack();
            Assert.IsTrue(item.StackSize == 2);
        }

        [TestMethod]
        public void RemoveFromStackTest()
        {
            var item = new InventoryItem(null);

            item.AddToStack();
            Assert.IsTrue(item.StackSize == 2);
            item.RemoveFromStack();
            Assert.IsTrue(item.StackSize == 1);
        }
    }
}
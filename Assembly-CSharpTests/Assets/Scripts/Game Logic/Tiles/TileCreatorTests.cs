using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class TileCreatorTests
    {
        [TestMethod]
        public void FindNeighbouringRoomsTest()
        {
            var tiles = new List<TileData>
            {
                new TileData(0, 0, TileType.Wall),
                new TileData(0, 1, TileType.Wall),
                new TileData(0, 2, TileType.Wall),
                new TileData(1, 0, TileType.Wall),
                new TileData(1, 1, TileType.Floor),
                new TileData(1, 2, TileType.Wall),
                new TileData(2, 0, TileType.Wall),
                new TileData(2, 1, TileType.Wall),
                new TileData(2, 2, TileType.Wall)
            };
            var room = new Room(1, tiles, null);

            var otherTiles = new List<TileData>
            {
                new TileData(3, 0, TileType.Wall),
                new TileData(3, 1, TileType.Wall),
                new TileData(3, 2, TileType.Wall),
                new TileData(4, 0, TileType.Wall),
                new TileData(4, 1, TileType.Floor),
                new TileData(4, 2, TileType.Wall),
                new TileData(5, 0, TileType.Wall),
                new TileData(5, 1, TileType.Wall),
                new TileData(5, 2, TileType.Wall)
            };
            var otherRoom = new Room(2, otherTiles, null);

            var roomList = new List<Room>
            {
                room,
                otherRoom
            };

            Assert.IsFalse(room.JointRooms.Contains(otherRoom));
            Assert.IsFalse(otherRoom.JointRooms.Contains(room));

            TileCreator.FindNeighbouringRooms(roomList);

            Assert.IsTrue(room.JointRooms.Contains(otherRoom));
            Assert.IsTrue(otherRoom.JointRooms.Contains(room));
        }
    }
}
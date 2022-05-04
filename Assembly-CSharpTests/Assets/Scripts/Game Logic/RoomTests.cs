using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class RoomTests
    {
        [TestMethod]
        public void RoomTest()
        {
            var tiles = new List<TileData>
            {
                new TileData(0, 0, TileType.Wall),
                new TileData(0, 1, TileType.Door),
                new TileData(0, 2, TileType.Wall),
                new TileData(1, 0, TileType.Wall),
                new TileData(1, 1, TileType.Floor),
                new TileData(1, 2, TileType.Wall),
                new TileData(2, 0, TileType.Wall),
                new TileData(2, 1, TileType.Door),
                new TileData(2, 2, TileType.Wall)
            };
            var room = new Room(1, tiles, null);
            Assert.IsNotNull(room);
            Assert.AreEqual(1, room.Id);
            Assert.AreEqual(0, room.XMin);
            Assert.AreEqual(0, room.YMin);
            Assert.AreEqual(2, room.XMax);
            Assert.AreEqual(2, room.YMax);

            var width = room.Width;
            var height = room.Height;
            Assert.AreEqual(1, width);
            Assert.AreEqual(1, height);

            var area = room.Area;
            Assert.AreEqual(1, area);

            var doorTiles = room.DoorTiles;
            Assert.AreEqual(2, doorTiles.Count);

            var tileData = new TileData(0, 0, TileType.Floor);
            Assert.IsFalse(room.Equals(tileData));

            tileData = null;
            Assert.IsFalse(room.Equals(tileData));
        }

        [TestMethod]
        public void SharesWallTest()
        {
            var tiles = new List<TileData>
            {
                new TileData(0, 0, TileType.Wall),
                new TileData(0, 1, TileType.Wall),
                new TileData(0, 2, TileType.Wall),
                new TileData(0, 3, TileType.Wall),
                new TileData(0, 4, TileType.Wall),
                new TileData(0, 5, TileType.Wall),
                new TileData(1, 0, TileType.Wall),
                new TileData(1, 1, TileType.Floor),
                new TileData(1, 2, TileType.Floor),
                new TileData(1, 3, TileType.Floor),
                new TileData(1, 4, TileType.Floor),
                new TileData(1, 5, TileType.Wall),
                new TileData(2, 0, TileType.Wall),
                new TileData(2, 1, TileType.Wall),
                new TileData(2, 2, TileType.Wall),
                new TileData(2, 3, TileType.Wall),
                new TileData(2, 4, TileType.Wall),
                new TileData(2, 5, TileType.Wall)
            };
            var room = new Room(1, tiles, null);

            var otherTiles = new List<TileData>
            {
                new TileData(2, 0, TileType.Wall),
                new TileData(2, 1, TileType.Wall),
                new TileData(2, 2, TileType.Wall),
                new TileData(2, 3, TileType.Wall),
                new TileData(2, 4, TileType.Wall),
                new TileData(2, 5, TileType.Wall),
                new TileData(3, 0, TileType.Wall),
                new TileData(3, 1, TileType.Floor),
                new TileData(3, 2, TileType.Floor),
                new TileData(3, 3, TileType.Floor),
                new TileData(3, 4, TileType.Floor),
                new TileData(3, 5, TileType.Wall),
                new TileData(4, 0, TileType.Wall),
                new TileData(4, 1, TileType.Wall),
                new TileData(4, 2, TileType.Wall),
                new TileData(4, 3, TileType.Wall),
                new TileData(4, 4, TileType.Wall),
                new TileData(4, 5, TileType.Wall)
            };
            var otherRoom = new Room(2, otherTiles, null);

            Assert.AreEqual(0, room.DoorPositions.Count);
            room.SharesWall(otherRoom);
            Assert.AreEqual(2, room.DoorPositions.Count);
        }

        [TestMethod]
        public void WallNearbyTestWest()
        {
            var tiles = new List<TileData>
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
            var room = new Room(1, tiles, null);

            var otherTiles = new List<TileData>
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
            var otherRoom = new Room(2, otherTiles, null);

            Assert.AreEqual(room.Tiles.Count, otherRoom.Tiles.Count);
            room.WallNearby(otherRoom);
            Assert.AreNotEqual(room.Tiles.Count, otherRoom.Tiles.Count);
            Assert.IsTrue(room.JointRooms.Contains(otherRoom));
            room.WallNearby(otherRoom);
        }

        [TestMethod]
        public void WallNearbyTestEast()
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

            Assert.AreEqual(room.Tiles.Count, otherRoom.Tiles.Count);
            room.WallNearby(otherRoom);
            Assert.AreNotEqual(room.Tiles.Count, otherRoom.Tiles.Count);
        }

        [TestMethod]
        public void WallNearbyTestNorth()
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
                new TileData(0, 3, TileType.Wall),
                new TileData(0, 4, TileType.Wall),
                new TileData(0, 5, TileType.Wall),
                new TileData(1, 3, TileType.Wall),
                new TileData(1, 4, TileType.Floor),
                new TileData(1, 5, TileType.Wall),
                new TileData(2, 3, TileType.Wall),
                new TileData(2, 4, TileType.Wall),
                new TileData(2, 5, TileType.Wall)
            };
            var otherRoom = new Room(2, otherTiles, null);

            Assert.AreEqual(room.Tiles.Count, otherRoom.Tiles.Count);
            room.WallNearby(otherRoom);
            Assert.AreNotEqual(room.Tiles.Count, otherRoom.Tiles.Count);
        }

        [TestMethod]
        public void WallNearbyTestSouth()
        {
            var tiles = new List<TileData>
            {
                new TileData(0, 3, TileType.Wall),
                new TileData(0, 4, TileType.Wall),
                new TileData(0, 5, TileType.Wall),
                new TileData(1, 3, TileType.Wall),
                new TileData(1, 4, TileType.Floor),
                new TileData(1, 5, TileType.Wall),
                new TileData(2, 3, TileType.Wall),
                new TileData(2, 4, TileType.Wall),
                new TileData(2, 5, TileType.Wall)
            };
            var room = new Room(1, tiles, null);

            var otherTiles = new List<TileData>
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
            var otherRoom = new Room(2, otherTiles, null);

            Assert.AreEqual(room.Tiles.Count, otherRoom.Tiles.Count);
            room.WallNearby(otherRoom);
            Assert.AreNotEqual(room.Tiles.Count, otherRoom.Tiles.Count);
        }

        [TestMethod]
        public void EqualsTest()
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
            var otherRoom = new Room(2, otherTiles, null);

            Assert.AreNotEqual(room, otherRoom);
        }

        [TestMethod]
        public void GetHashCodeTest()
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

            Assert.AreNotEqual(room.GetHashCode(), otherRoom.GetHashCode());
        }
    }
}
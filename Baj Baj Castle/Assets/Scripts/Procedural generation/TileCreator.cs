using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCreator
{
    private Dictionary<string, Tile> tiles;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;
    private Tilemap collisionTilemap;

    public TileCreator(Tilemap floorTilemap, Tilemap decorationTilemap, Tilemap collisionTilemap)
    {
        tiles = GameAssets.instance.tiles;
        this.floorTilemap = floorTilemap;
        this.decorationTilemap = decorationTilemap;
        this.collisionTilemap = collisionTilemap;
    }

    // Puts room cells into a list of rooms with tiles
    public List<Room> CreateRooms(List<Cell> roomCells)
    {
        List<Room> rooms = new List<Room>();
        foreach (var cell in roomCells)
        {
            var halfWidth = cell.Width / 2;
            var halfHeight = cell.Height / 2;

            int startingX = Mathf.RoundToInt(cell.Position.x) - halfWidth;
            int startingY = Mathf.RoundToInt(cell.Position.y) - halfHeight;
            int endingX = startingX + cell.Width;
            int endingY = startingY + cell.Height;

            var tilesList = new List<TileData>();
            for (var x = startingX; x < endingX; x++)
            {
                for (var y = startingY; y < endingY; y++)
                {
                    tilesList.Add(new TileData(x, y, TileType.None));
                }
            }
            rooms.Add(new Room(tilesList));
        }
        return rooms;
    }

    public void UpdateTiles(List<Room> rooms)
    {
        for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++) // iterate through rooms
        {
            var room = rooms[roomIndex];
            Debug.Log("Filling room " + roomIndex);

            // Separate room tiles into floors and walls
            foreach (var tile in room.Tiles)
            {
                if (tile.X == room.X_Min || tile.X == room.X_Max || tile.Y == room.Y_Min || tile.Y == room.Y_Max)
                {
                    tile.Type = TileType.Wall;
                }
                else
                {
                    tile.Type = TileType.Floor;
                }
            }

            // Take all tiles which are TileType.Wall
            var wallTiles = room.Tiles.Where(x => x.Type == TileType.Wall);
            foreach (var tile in wallTiles)
            {
                // check all directions for any tile
                var hasWest = room.Tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
                var hasEast = room.Tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
                var hasNorth = room.Tiles.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
                var hasSouth = room.Tiles.Any(t => t.X == tile.X && t.Y == tile.Y - 1);

                var location = new Vector3Int(tile.X, tile.Y, 0);
                collisionTilemap.SetTile(location, tiles["Collision"]);
                if (!hasWest && hasEast && hasNorth && hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallW"]);
                }
                else if (hasWest && !hasEast && hasNorth && hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallE"]);
                }
                else if (hasWest && hasEast && !hasNorth && hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallN"]);
                }
                else if (hasWest && hasEast && hasNorth && !hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallS"]);
                }
                else if (!hasWest && hasEast && !hasNorth && hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallNW"]);
                }
                else if (hasWest && !hasEast && !hasNorth && hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallNE"]);
                }
                else if (!hasWest && hasEast && hasNorth && !hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallSW"]);
                }
                else if (hasWest && !hasEast && hasNorth && !hasSouth)
                {
                    floorTilemap.SetTile(location, tiles["WallSE"]);
                }
                else
                {
                    floorTilemap.SetTile(location, tiles["Collision"]);
                }
            }
        }
    }

    // public void UpdateTiles(List<List<(int, int)>> rooms)
    // {
    //     for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++)
    //     {
    //         var room = rooms[roomIndex];
    //         Debug.Log("Filling room " + roomIndex);
    //         for (int roomTileIndex = 0; roomTileIndex < room.Count; roomTileIndex++)
    //         {
    //             var coordinates = room[roomTileIndex];
    //             var x = coordinates.Item1;
    //             var y = coordinates.Item2;

    //             var tile = floorTilemap.GetTile(new Vector3Int(x, y, 0));
    //             if (tile.name != "Floor")
    //             {
    //                 continue;
    //             }

    //             // check what TileType it is by checking for all neighbours
    //             var north = floorTilemap.HasTile(new Vector3Int(x, y + 1, 0));
    //             var south = floorTilemap.HasTile(new Vector3Int(x, y - 1, 0));
    //             var west = floorTilemap.HasTile(new Vector3Int(x - 1, y, 0));
    //             var east = floorTilemap.HasTile(new Vector3Int(x + 1, y, 0));

    //             var northWest = floorTilemap.HasTile(new Vector3Int(x - 1, y + 1, 0));
    //             var northEast = floorTilemap.HasTile(new Vector3Int(x + 1, y + 1, 0));
    //             var southWest = floorTilemap.HasTile(new Vector3Int(x - 1, y - 1, 0));
    //             var southEast = floorTilemap.HasTile(new Vector3Int(x + 1, y - 1, 0));

    //             if (!north && !northEast && east && southEast && south && southWest && west && !northWest) // north
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallN"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), tiles["FloorN"]);
    //                 floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), tiles["WallTopN"]);
    //             }
    //             else if (north && northEast && east && !southEast && !south && !southWest && west && northWest) // south
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallS"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), tiles["FloorS"]);
    //                 floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), tiles["WallTopS"]);
    //             }
    //             else if (north && !northEast && !east && !southEast && south && southWest && west && northWest) // east
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallE"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x - 1, y, 0), tiles["FloorE"]);
    //                 floorTilemap.SetTile(new Vector3Int(x + 1, y, 0), tiles["WallTopE"]);
    //             }
    //             else if (north && northEast && east && southEast && south && !southWest && !west && !northWest) // west
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallW"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x + 1, y, 0), tiles["FloorW"]);
    //                 floorTilemap.SetTile(new Vector3Int(x - 1, y, 0), tiles["WallTopW"]);
    //             }
    //             else if (!north && !northEast && east && southEast && south && !southWest && !west && !northWest) // northWest
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallNW"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), tiles["FloorNW"]);
    //                 floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), tiles["WallTopNWN"]);
    //                 floorTilemap.SetTile(new Vector3Int(x - 1, y + 1, 0), tiles["WallTopNWC"]);
    //                 floorTilemap.SetTile(new Vector3Int(x - 1, y, 0), tiles["WallTopNWW"]);
    //             }
    //             else if (!north && !northEast && !east && !southEast && south && southWest && west && !northWest) // northEast
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallNE"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), tiles["FloorNE"]);
    //                 floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), tiles["WallTopNEN"]);
    //                 floorTilemap.SetTile(new Vector3Int(x + 1, y + 1, 0), tiles["WallTopNEC"]);
    //                 floorTilemap.SetTile(new Vector3Int(x + 1, y, 0), tiles["WallTopNEE"]);
    //             }
    //             else if (north && northEast && east && !southEast && !south && !southWest && !west && !northWest) // southWest
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallSW"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), tiles["FloorSW"]);
    //                 floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), tiles["WallTopSWS"]);
    //                 floorTilemap.SetTile(new Vector3Int(x - 1, y - 1, 0), tiles["WallTopSWC"]);
    //                 floorTilemap.SetTile(new Vector3Int(x - 1, y, 0), tiles["WallTopSWW"]);
    //             }
    //             else if (north && !northEast && !east && !southEast && !south && !southWest && west && northWest) // southEast
    //             {
    //                 floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["WallSE"]);
    //                 collisionTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Collision"]);
    //                 // floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), tiles["FloorSE"]);
    //                 floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), tiles["WallTopSES"]);
    //                 floorTilemap.SetTile(new Vector3Int(x + 1, y - 1, 0), tiles["WallTopSEC"]);
    //                 floorTilemap.SetTile(new Vector3Int(x + 1, y, 0), tiles["WallTopSEE"]);
    //             }
    //             // else // center
    //             // {
    //             //     var tile = floorTilemap.GetTile(new Vector3Int(x, y, 0));
    //             //     if(tile.name == "")
    //             //     floorTilemap.SetTile(new Vector3Int(x, y, 0), tiles["Floor"]);
    //             // }
    //         }
    //     }
    // }

    // Puts hallways cells into a tiledata list
    public List<TileData> CreateHallways(List<Cell> hallwayCells)
    {
        var tileList = new List<TileData>();
        foreach (var cell in hallwayCells)
        {
            var x = (int)cell.Position.x - 1;
            var y = (int)cell.Position.y - 1;
            // if tile is not empty
            if (floorTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
            {
                tileList.Add(new TileData(x, y, TileType.None));
            }
        }
        return tileList;
    }
}

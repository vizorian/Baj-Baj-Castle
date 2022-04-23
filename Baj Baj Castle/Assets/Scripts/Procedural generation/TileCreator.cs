using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCreator
{
    private Dictionary<string, Tile> tileDict;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;
    private Tilemap collisionTilemap;

    public TileCreator(Tilemap floorTilemap, Tilemap decorationTilemap, Tilemap collisionTilemap)
    {
        tileDict = GameAssets.Instance.tiles;
        this.floorTilemap = floorTilemap;
        this.decorationTilemap = decorationTilemap;
        this.collisionTilemap = collisionTilemap;
    }

    // Puts room cells into a list of rooms with tiles
    public List<Room> CreateRooms(List<Cell> roomCells)
    {
        List<Room> rooms = new List<Room>();
        for (int i = 0; i < roomCells.Count; i++)
        {
            Cell cell = roomCells[i];
            var halfWidth = cell.Width / 2;
            var halfHeight = cell.Height / 2;

            int startingX = Mathf.RoundToInt(cell.Position.x) - halfWidth;
            int startingY = Mathf.RoundToInt(cell.Position.y) - halfHeight;
            int endingX = startingX + cell.Width;
            int endingY = startingY + cell.Height;

            var tilesToAdd = new List<TileData>();
            for (var x = startingX; x < endingX; x++)
            {
                for (var y = startingY; y < endingY; y++)
                {
                    tilesToAdd.Add(new TileData(x, y, TileType.None));
                }
            }
            rooms.Add(new Room(i, tilesToAdd));
        }
        return rooms;
    }

    public void FindNeighbouringRooms(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            for (int j = 0; j < rooms.Count; j++)
            {
                if (i != j)
                {
                    Room otherRoom = rooms[j];
                    room.WallNearby(otherRoom);
                    room.SharesWall(otherRoom);
                }
            }
        }
    }

    public void CreateTiles(List<Room> rooms, List<TileData> hallwayTiles)
    {
        // Setting tile types for all tiles
        SetTileTypes(rooms, hallwayTiles);

        var allTiles = rooms.SelectMany(x => x.Tiles).ToList();
        allTiles.AddRange(hallwayTiles);

        var floorTiles = allTiles.Where(x => x.Type == TileType.Floor).ToList();
        var wallTiles = allTiles.Where(x => x.Type == TileType.Wall).ToList();
        var doorTiles = allTiles.Where(x => x.Type == TileType.Door).ToList();

        Debug.Log("Doors");
        SetTiles(doorTiles);
        Debug.Log("Floors");
        SetTiles(floorTiles);
        Debug.Log("Walls");
        SetTiles(wallTiles, allTiles);

    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        decorationTilemap.ClearAllTiles();
        collisionTilemap.ClearAllTiles();
    }

    private void SetTileTypes(List<Room> rooms, List<TileData> hallwayTiles)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            FloatingText.Create(i.ToString(), Color.red, new Vector3((room.X_Max + room.X_Min) * LevelGenerator.CELL_SIZE / 2, (room.Y_Max + room.Y_Min) * LevelGenerator.CELL_SIZE / 2, 0), 4f, 100f, 0f);
            Debug.Log("Setting room tiles for room " + i);
            foreach (var tile in room.Tiles)
            {
                // TODO check for doors
                if (tile.Type != TileType.None)
                {
                    continue;
                }

                if (!room.Tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y + 1) // If there's no tile diagonally adjacent to this one, it's a wall
                    || !room.Tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y + 1)
                    || !room.Tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y - 1)
                    || !room.Tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y - 1))
                {
                    if (room.DoorPositions.Any(door => Mathf.RoundToInt(door.x) == tile.X && Mathf.RoundToInt(door.y) == tile.Y))
                    {
                        tile.Type = TileType.Door;
                    }
                    else
                    {
                        tile.Type = TileType.Wall;
                    }
                }
                else
                {
                    tile.Type = TileType.Floor;
                }

            }
        }

        foreach (var tile in hallwayTiles)
        {
            if (!hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y + 1) // If there's no tile diagonally adjacent to this one, it's a wall
                || !hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y + 1)
                || !hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y - 1)
                || !hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y - 1))
            {
                tile.Type = TileType.Wall;
            }
            else
            {
                tile.Type = TileType.Floor;
            }
        }

        //for each room, find hallway tiles of type Wall
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            var roomWalls = room.Tiles.Where(t => t.Type == TileType.Wall).ToList();

            // West wall
            var westNearbyTiles = new List<TileData>();
            var westTiles = new List<TileData>();
            foreach (var wall in roomWalls.Where(w => w.X == room.X_Min).ToList())
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(x => x.X == wall.X - 1 && x.Y == wall.Y);
                if (nearbyTile != null)
                {
                    westNearbyTiles.Add(nearbyTile);
                    westTiles.Add(wall);
                }
            }
            if (westNearbyTiles.Count > 3)
            {
                var middle = westNearbyTiles.Count / 2;
                westNearbyTiles[middle].Type = TileType.Floor;
                westNearbyTiles[middle - 1].Type = TileType.Floor;
                westTiles[middle].Type = TileType.Floor;
                westTiles[middle - 1].Type = TileType.Floor;
            }

            // East wall
            var eastNearbyTiles = new List<TileData>();
            var eastTiles = new List<TileData>();
            foreach (var wall in roomWalls.Where(w => w.X == room.X_Max).ToList())
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(x => x.X == wall.X + 1 && x.Y == wall.Y);
                if (nearbyTile != null)
                {
                    eastNearbyTiles.Add(nearbyTile);
                    eastTiles.Add(wall);
                }
            }
            if (eastNearbyTiles.Count > 3)
            {
                var middle = eastNearbyTiles.Count / 2;
                eastNearbyTiles[middle].Type = TileType.Floor;
                eastNearbyTiles[middle - 1].Type = TileType.Floor;
                eastTiles[middle].Type = TileType.Floor;
                eastTiles[middle - 1].Type = TileType.Floor;
            }

            // North wall
            var northNearbyTiles = new List<TileData>();
            var northTiles = new List<TileData>();
            foreach (var wall in roomWalls.Where(w => w.Y == room.Y_Max).ToList())
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(x => x.X == wall.X && x.Y == wall.Y + 1);
                if (nearbyTile != null)
                {
                    northNearbyTiles.Add(nearbyTile);
                    northTiles.Add(wall);
                }
            }
            if (northNearbyTiles.Count > 3)
            {
                var middle = northNearbyTiles.Count / 2;
                northTiles[middle].Type = TileType.Door;
                northTiles[middle - 1].Type = TileType.Door;
                northNearbyTiles[middle].Type = TileType.Floor;
                northNearbyTiles[middle - 1].Type = TileType.Floor;
            }

            // South wall
            var southNearbyTiles = new List<TileData>();
            var southTiles = new List<TileData>();
            foreach (var wall in roomWalls.Where(w => w.Y == room.Y_Min).ToList())
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(x => x.X == wall.X && x.Y == wall.Y - 1);
                if (nearbyTile != null)
                {
                    southNearbyTiles.Add(nearbyTile);
                    southTiles.Add(wall);
                }
            }
            if (southNearbyTiles.Count > 3)
            {
                var middle = southNearbyTiles.Count / 2;
                southTiles[middle].Type = TileType.Door;
                southTiles[middle - 1].Type = TileType.Door;
                southNearbyTiles[middle].Type = TileType.Floor;
                southNearbyTiles[middle - 1].Type = TileType.Floor;
            }
        }
    }

    private void SetTiles(List<TileData> tiles, List<TileData> tilesToCheck = null)
    {
        if (tilesToCheck == null)
        {
            tilesToCheck = tiles;
        }

        foreach (var tile in tiles)
        {
            var location = new Vector3Int(tile.X, tile.Y, 0);
            TileType tileType = TileType.None;
            string baseName = "";
            switch (tile.Type)
            {
                case TileType.Wall:
                    tileType = TileType.Wall;
                    baseName = "Wall";
                    collisionTilemap.SetTile(location, tileDict["Collision"]);
                    break;
                case TileType.Floor:
                    tileType = TileType.Floor;
                    baseName = "Floor";
                    break;
                case TileType.Door:
                    tileType = TileType.Door;
                    floorTilemap.SetTile(location, tileDict["Door"]);
                    continue;
            }

            bool hasWest, hasEast, hasNorth, hasSouth;

            // check all directionss for tiles
            hasWest = tilesToCheck.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
            hasEast = tilesToCheck.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
            hasNorth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
            hasSouth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y - 1);

            var directionalName = ProcessTile(hasWest, hasEast, hasNorth, hasSouth, tileType);
            if (directionalName == "" && tileType == TileType.Wall) // corner
            {
                hasWest = tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
                hasEast = tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
                hasNorth = tiles.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
                hasSouth = tiles.Any(t => t.X == tile.X && t.Y == tile.Y - 1);
                directionalName = ProcessTile(hasWest, hasEast, hasNorth, hasSouth, tileType);
                if (directionalName == "") // straight
                {
                    hasWest = tilesToCheck.Any(t => t.X == tile.X - 1 && t.Y == tile.Y && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    hasEast = tilesToCheck.Any(t => t.X == tile.X + 1 && t.Y == tile.Y && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    hasNorth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y + 1 && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    hasSouth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y - 1 && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    directionalName = ProcessTile(hasWest, hasEast, hasNorth, hasSouth, tileType);
                }
            }
            try
            {
                floorTilemap.SetTile(location, tileDict[baseName + directionalName]);

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log("Exception with tile " + baseName + directionalName);
            }
        }
    }

    private string ProcessTile(bool W, bool E, bool N, bool S, TileType tileType)
    {
        if (!W && E && N && S)
        {
            return "W";
        }
        else if (W && !E && N && S)
        {
            return "E";
        }
        else if (W && E && !N && S)
        {
            return "N";
        }
        else if (W && E && N && !S)
        {
            return "S";
        }
        else if (!W && E && !N && S)
        {
            return "NW";
        }
        else if (W && !E && !N && S)
        {
            return "NE";
        }
        else if (!W && E && N && !S)
        {
            return "SW";
        }
        else if (W && !E && N && !S)
        {
            return "SE";
        }
        else if (tileType == TileType.Wall)
        {
            if (!W && !E && N && S)
            {
                return "NS";
            }
            else if (W && E && !N && !S)
            {
                return "WE";
            }
        }

        return "";
    }

    // Puts hallways cells into a tiledata list
    public List<TileData> CreateHallways(List<Cell> hallwayCells, List<Room> rooms)
    {
        var tileList = new List<TileData>();
        foreach (var cell in hallwayCells)
        {
            var x = (int)cell.Position.x - 1;
            var y = (int)cell.Position.y - 1;

            var tile = new TileData(x, y, TileType.None);
            // if tile is not within any room
            if (!rooms.Any(r => r.Tiles.Contains(tile)))
            {
                tileList.Add(new TileData(x, y, TileType.None));
            }
        }
        return tileList;
    }
}

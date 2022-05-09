using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCreator
{
    private readonly Tilemap collisionTilemap;
    private readonly Tilemap decorationTilemap;
    private readonly Tilemap floorTilemap;
    private readonly bool isDebug;
    private readonly Dictionary<string, Tile> tileDict;

    public TileCreator(Tilemap floorTilemap, Tilemap decorationTilemap, Tilemap collisionTilemap, bool isDebug)
    {
        this.floorTilemap = floorTilemap;
        this.decorationTilemap = decorationTilemap;
        this.collisionTilemap = collisionTilemap;
        this.isDebug = isDebug;
        tileDict = GameAssets.Instance.Tiles;
    }

    // Puts room cells into a list of rooms with tiles
    public List<Room> CreateRooms(List<Cell> roomCells)
    {
        var rooms = new List<Room>();
        for (var i = 0; i < roomCells.Count; i++)
        {
            var cell = roomCells[i];
            var halfWidth = cell.Width / 2;
            var halfHeight = cell.Height / 2;

            var startingX = Mathf.RoundToInt(cell.Position.x) - halfWidth;
            var startingY = Mathf.RoundToInt(cell.Position.y) - halfHeight;
            var endingX = startingX + cell.Width;
            var endingY = startingY + cell.Height;

            var tilesToAdd = new List<TileData>();
            for (var x = startingX; x < endingX; x++)
                for (var y = startingY; y < endingY; y++)
                    tilesToAdd.Add(new TileData(x, y, TileType.None));
            rooms.Add(new Room(i, tilesToAdd, this));
        }

        return rooms;
    }

    // Finds all shared and nearby rooms
    public static void FindNeighbouringRooms(List<Room> rooms)
    {
        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            for (var j = 0; j < rooms.Count; j++)
                if (i != j)
                {
                    var otherRoom = rooms[j];
                    room.WallNearby(otherRoom);
                }
        }

        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            for (var j = 0; j < rooms.Count; j++)
                if (i != j)
                {
                    var otherRoom = rooms[j];
                    room.SharesWall(otherRoom);
                }
        }
    }

    // Create tiles for rooms and hallways
    public void CreateTiles(List<Room> rooms, List<TileData> hallwayTiles)
    {
        // Setting tile types for all tiles
        SetTileTypes(rooms, hallwayTiles);

        // Getting all tiles for comparison
        var allTiles = rooms.SelectMany(x => x.Tiles).ToList();
        allTiles.AddRange(hallwayTiles);

        // Getting different tile types
        var floorTiles = allTiles.Where(x => x.Type == TileType.Floor).ToList();
        var wallTiles = allTiles.Where(x => x.Type == TileType.Wall).ToList();
        var doorTiles = allTiles.Where(x => x.Type == TileType.Door).ToList();

        // Setting tiles for floors with no references
        SetTiles(floorTiles, null);
        // Setting tiles for walls with all tiles as a reference
        SetTiles(wallTiles, allTiles);
        // Setting tiles for doors with no references
        SetTiles(doorTiles, null);
    }

    // Cleanup logic
    public void Cleanup()
    {
        floorTilemap.ClearAllTiles();
        decorationTilemap.ClearAllTiles();
        collisionTilemap.ClearAllTiles();
    }

    // Sets tile types for room and hallway tiles
    private void SetTileTypes(List<Room> rooms, List<TileData> hallwayTiles)
    {
        // Setting room tile types
        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (isDebug)
                FloatingText.Create(i.ToString(), Color.red,
                    new Vector3((room.XMax + room.XMin) * LevelGenerator.CELL_SIZE / 2,
                        (room.YMax + room.YMin) * LevelGenerator.CELL_SIZE / 2, 0), 4f, 100f, 0f);

            foreach (var tile in room.Tiles)
            {
                if (tile.Type != TileType.None) continue;

                // Determine tile type
                if (!room.Tiles.Any(t =>
                        t.X == tile.X - 1 &&
                        t.Y == tile.Y + 1)
                    || !room.Tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y + 1)
                    || !room.Tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y - 1)
                    || !room.Tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y - 1)) // If there's no tile diagonally adjacent to this one, it's a wall
                {
                    tile.Type = room.DoorPositions.Any(door =>
                        Mathf.RoundToInt(door.x) == tile.X && Mathf.RoundToInt(door.y) == tile.Y) ? TileType.Door : TileType.Wall;
                }
                else
                {
                    tile.Type = TileType.Floor;
                }
            }
        }

        // Setting hallway tile types
        foreach (var tile in hallwayTiles)
            if (!hallwayTiles.Any(t =>
                    t.X == tile.X - 1 &&
                    t.Y == tile.Y + 1)
                || !hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y + 1)
                || !hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y - 1)
                || !hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y - 1)) // If there's no tile diagonally adjacent to this one, it's a wall
                tile.Type = TileType.Wall;
            else
                tile.Type = TileType.Floor;

        // For each room, find nearby hallway tiles and group them if they are separated
        // This is done for each side of the room
        // All groups are then checked to see if they fit a door
        foreach (var room in rooms)
        {
            // Preparation
            var roomWalls = room.Tiles.Where(t => t.Type == TileType.Wall).ToList();
            var tempNearbyTiles = new List<TileData>();
            var tempRoomTiles = new List<TileData>();
            var nearbyGroup = new List<TileData>();
            var roomGroup = new List<TileData>();

            // West wall
            var westNearbyTileGroups = new List<List<TileData>>();
            var westTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.X == room.XMin).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X - 1 && t.Y == wall.Y);
                if (nearbyTile == null) continue;
                tempNearbyTiles.Add(nearbyTile);
                tempRoomTiles.Add(wall);
            }

            for (var i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].X != tempNearbyTiles[i + 1].X ||
                    Mathf.Abs(tempNearbyTiles[i].Y - tempNearbyTiles[i + 1].Y) > 1) // if tile out of range
                {
                    // create new group
                    westNearbyTileGroups.Add(nearbyGroup);
                    westTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1) isLast = true;
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;
                }

                if (isLast && i != tempNearbyTiles.Count - 1)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    westNearbyTileGroups.Add(nearbyGroup);
                    westTileGroups.Add(roomGroup);
                }
            }

            for (var i = 0; i < westNearbyTileGroups.Count; i++)
                if (westNearbyTileGroups[i].Count > 3)
                {
                    var middle = westNearbyTileGroups[i].Count / 2;
                    westNearbyTileGroups[i][middle].Type = TileType.Floor;
                    westNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    westTileGroups[i][middle].Type = TileType.Door;
                    westTileGroups[i][middle - 1].Type = TileType.Door;
                }

            // East wall
            tempNearbyTiles = new List<TileData>();
            tempRoomTiles = new List<TileData>();
            nearbyGroup = new List<TileData>();
            roomGroup = new List<TileData>();
            var eastNearbyTileGroups = new List<List<TileData>>();
            var eastTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.X == room.XMax).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X + 1 && t.Y == wall.Y);
                if (nearbyTile == null) continue;
                tempNearbyTiles.Add(nearbyTile);
                tempRoomTiles.Add(wall);
            }

            for (var i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].X != tempNearbyTiles[i + 1].X ||
                    Mathf.Abs(tempNearbyTiles[i].Y - tempNearbyTiles[i + 1].Y) > 1) // if tile out of range
                {
                    // create new group
                    eastNearbyTileGroups.Add(nearbyGroup);
                    eastTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1) isLast = true;
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;
                }

                if (isLast && i != tempNearbyTiles.Count - 1)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    eastNearbyTileGroups.Add(nearbyGroup);
                    eastTileGroups.Add(roomGroup);
                }
            }

            for (var i = 0; i < eastNearbyTileGroups.Count; i++)
                if (eastNearbyTileGroups[i].Count > 3)
                {
                    var middle = eastNearbyTileGroups[i].Count / 2;
                    eastNearbyTileGroups[i][middle].Type = TileType.Floor;
                    eastNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    eastTileGroups[i][middle].Type = TileType.Door;
                    eastTileGroups[i][middle - 1].Type = TileType.Door;
                }

            // North wall
            tempNearbyTiles = new List<TileData>();
            tempRoomTiles = new List<TileData>();
            nearbyGroup = new List<TileData>();
            roomGroup = new List<TileData>();
            var northNearbyTileGroups = new List<List<TileData>>();
            var northTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.Y == room.YMax).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X && t.Y == wall.Y + 1);
                if (nearbyTile == null) continue;
                tempNearbyTiles.Add(nearbyTile);
                tempRoomTiles.Add(wall);
            }

            for (var i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].Y != tempNearbyTiles[i + 1].Y ||
                    Mathf.Abs(tempNearbyTiles[i].X - tempNearbyTiles[i + 1].X) > 1) // if tile out of range
                {
                    // create new group
                    northNearbyTileGroups.Add(nearbyGroup);
                    northTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1) isLast = true;
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;
                }

                if (isLast && i != tempNearbyTiles.Count - 1)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    northNearbyTileGroups.Add(nearbyGroup);
                    northTileGroups.Add(roomGroup);
                }
            }

            for (var i = 0; i < northNearbyTileGroups.Count; i++)
                if (northNearbyTileGroups[i].Count > 3)
                {
                    var middle = northNearbyTileGroups[i].Count / 2;
                    northNearbyTileGroups[i][middle].Type = TileType.Floor;
                    northNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    northTileGroups[i][middle].Type = TileType.Door;
                    northTileGroups[i][middle - 1].Type = TileType.Door;
                }

            // South wall
            tempNearbyTiles = new List<TileData>();
            tempRoomTiles = new List<TileData>();
            nearbyGroup = new List<TileData>();
            roomGroup = new List<TileData>();
            var southNearbyTileGroups = new List<List<TileData>>();
            var southTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.Y == room.YMin).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X && t.Y == wall.Y - 1);
                if (nearbyTile == null) continue;
                tempNearbyTiles.Add(nearbyTile);
                tempRoomTiles.Add(wall);
            }

            for (var i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].Y != tempNearbyTiles[i + 1].Y ||
                    Mathf.Abs(tempNearbyTiles[i].X - tempNearbyTiles[i + 1].X) > 1) // if tile out of range
                {
                    // create new group
                    southNearbyTileGroups.Add(nearbyGroup);
                    southTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1) isLast = true;
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;
                }

                if (isLast && i != tempNearbyTiles.Count - 1)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    southNearbyTileGroups.Add(nearbyGroup);
                    southTileGroups.Add(roomGroup);
                }
            }

            for (var i = 0; i < southNearbyTileGroups.Count; i++)
                if (southNearbyTileGroups[i].Count > 3)
                {
                    var middle = southNearbyTileGroups[i].Count / 2;
                    southNearbyTileGroups[i][middle].Type = TileType.Floor;
                    southNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    southTileGroups[i][middle].Type = TileType.Door;
                    southTileGroups[i][middle - 1].Type = TileType.Door;
                }
        }
    }

    // Add door tile to collision layer
    public void AddToCollisionLayer(TileData doorTile)
    {
        collisionTilemap.SetTile(new Vector3Int(doorTile.X, doorTile.Y, 0), tileDict["Collision"]);
    }

    // Remove door tile from collision layer
    public void RemoveFromCollisionLayer(TileData doorTile)
    {
        collisionTilemap.SetTile(new Vector3Int(doorTile.X, doorTile.Y, 0), null);
    }

    // Override tile with new tile
    public void OverrideTile(TileData tile, string type)
    {
        floorTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), tileDict[type]);
    }

    // Set tile types for list of tiles
    // Also use tilesToCheck for correct texture mapping
    private void SetTiles(List<TileData> tiles, List<TileData> tilesToCheck)
    {
        // If tilesToCheck is empty, use self as reference
        tilesToCheck ??= tiles;

        foreach (var tile in tiles)
        {
            var location = new Vector3Int(tile.X, tile.Y, 0);
            var tileType = TileType.None;
            var baseName = "";
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
                    floorTilemap.SetTile(location, tileDict["Door"]);
                    continue;
            }

            // check all directions for any reference tiles
            var hasWest = tilesToCheck.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
            var hasEast = tilesToCheck.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
            var hasNorth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
            var hasSouth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y - 1);

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
                    hasWest = tilesToCheck.Any(t =>
                        t.X == tile.X - 1 && t.Y == tile.Y && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    hasEast = tilesToCheck.Any(t =>
                        t.X == tile.X + 1 && t.Y == tile.Y && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    hasNorth = tilesToCheck.Any(t =>
                        t.X == tile.X && t.Y == tile.Y + 1 && (t.Type == TileType.Wall || t.Type == TileType.Door));
                    hasSouth = tilesToCheck.Any(t =>
                        t.X == tile.X && t.Y == tile.Y - 1 && (t.Type == TileType.Wall || t.Type == TileType.Door));
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

    // Get tile directional name, based on surrounding tiles
    private string ProcessTile(bool w, bool e, bool n, bool s, TileType tileType)
    {
        if (!w && e && n && s) return "W";

        if (w && !e && n && s) return "E";

        if (w && e && !n && s) return "N";

        if (w && e && n && !s) return "S";

        if (!w && e && !n && s) return "NW";

        if (w && !e && !n && s) return "NE";

        if (!w && e && n && !s) return "SW";

        if (w && !e && n && !s) return "SE";

        if (tileType == TileType.Wall)
        {
            if (!w && !e && n && s)
                return "NS";
            if (w && e && !n && !s) return "WE";
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
            if (!rooms.Any(r => r.Tiles.Contains(tile))) tileList.Add(new TileData(x, y, TileType.None));
        }

        return tileList;
    }
}
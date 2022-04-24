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
    private bool isDebug;

    public TileCreator(Tilemap floorTilemap, Tilemap decorationTilemap, Tilemap collisionTilemap, bool isDebug)
    {
        this.floorTilemap = floorTilemap;
        this.decorationTilemap = decorationTilemap;
        this.collisionTilemap = collisionTilemap;
        this.isDebug = isDebug;
        tileDict = GameAssets.Instance.tiles;
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
                }
            }
        }
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            for (int j = 0; j < rooms.Count; j++)
            {
                if (i != j)
                {
                    Room otherRoom = rooms[j];
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

        SetTiles(doorTiles);
        SetTiles(floorTiles);
        SetTiles(wallTiles, allTiles);
    }

    public void Cleanup()
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
            if (isDebug)
            {
                FloatingText.Create(i.ToString(), Color.red, new Vector3((room.X_Max + room.X_Min) * LevelGenerator.CELL_SIZE / 2, (room.Y_Max + room.Y_Min) * LevelGenerator.CELL_SIZE / 2, 0), 4f, 100f, 0f);
            }
            foreach (var tile in room.Tiles)
            {
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
        foreach (var room in rooms)
        {
            // Preparation
            var roomWalls = room.Tiles.Where(t => t.Type == TileType.Wall).ToList();
            var tempNearbyTiles = new List<TileData>();
            var tempRoomTiles = new List<TileData>();
            List<TileData> nearbyGroup = new List<TileData>();
            List<TileData> roomGroup = new List<TileData>();

            // West wall
            var westNearbyTileGroups = new List<List<TileData>>();
            var westTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.X == room.X_Min).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X - 1 && t.Y == wall.Y);
                if (nearbyTile != null) // if tile exists
                {
                    tempNearbyTiles.Add(nearbyTile);
                    tempRoomTiles.Add(wall);
                }
            }
            for (int i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].X != tempNearbyTiles[i + 1].X || Mathf.Abs(tempNearbyTiles[i].Y - tempNearbyTiles[i + 1].Y) > 1) // if tile out of range
                {
                    // create new group
                    westNearbyTileGroups.Add(nearbyGroup);
                    westTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1)
                    {
                        isLast = true;
                    }
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;

                }
                if (isLast)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    westNearbyTileGroups.Add(nearbyGroup);
                    westTileGroups.Add(roomGroup);
                }
            }
            for (int i = 0; i < westNearbyTileGroups.Count; i++)
            {
                if (westNearbyTileGroups[i].Count > 3)
                {
                    var middle = westNearbyTileGroups[i].Count / 2;
                    westNearbyTileGroups[i][middle].Type = TileType.Floor;
                    westNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    westTileGroups[i][middle].Type = TileType.Door;
                    westTileGroups[i][middle - 1].Type = TileType.Door;
                }
            }

            // East wall
            tempNearbyTiles = new List<TileData>();
            tempRoomTiles = new List<TileData>();
            nearbyGroup = new List<TileData>();
            roomGroup = new List<TileData>();
            var eastNearbyTileGroups = new List<List<TileData>>();
            var eastTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.X == room.X_Max).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X + 1 && t.Y == wall.Y);
                if (nearbyTile != null) // if tile exists
                {
                    tempNearbyTiles.Add(nearbyTile);
                    tempRoomTiles.Add(wall);
                }
            }
            for (int i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].X != tempNearbyTiles[i + 1].X || Mathf.Abs(tempNearbyTiles[i].Y - tempNearbyTiles[i + 1].Y) > 1) // if tile out of range
                {
                    // create new group
                    eastNearbyTileGroups.Add(nearbyGroup);
                    eastTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1)
                    {
                        isLast = true;
                    }
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;

                }
                if (isLast)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    eastNearbyTileGroups.Add(nearbyGroup);
                    eastTileGroups.Add(roomGroup);
                }
            }
            for (int i = 0; i < eastNearbyTileGroups.Count; i++)
            {
                if (eastNearbyTileGroups[i].Count > 3)
                {
                    var middle = eastNearbyTileGroups[i].Count / 2;
                    eastNearbyTileGroups[i][middle].Type = TileType.Floor;
                    eastNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    eastTileGroups[i][middle].Type = TileType.Door;
                    eastTileGroups[i][middle - 1].Type = TileType.Door;
                }
            }

            // North wall
            tempNearbyTiles = new List<TileData>();
            tempRoomTiles = new List<TileData>();
            nearbyGroup = new List<TileData>();
            roomGroup = new List<TileData>();
            var northNearbyTileGroups = new List<List<TileData>>();
            var northTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.Y == room.Y_Max).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X && t.Y == wall.Y + 1);
                if (nearbyTile != null) // if tile exists
                {
                    tempNearbyTiles.Add(nearbyTile);
                    tempRoomTiles.Add(wall);
                }
            }
            for (int i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].Y != tempNearbyTiles[i + 1].Y || Mathf.Abs(tempNearbyTiles[i].X - tempNearbyTiles[i + 1].X) > 1) // if tile out of range
                {
                    // create new group
                    northNearbyTileGroups.Add(nearbyGroup);
                    northTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1)
                    {
                        isLast = true;
                    }
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;

                }
                if (isLast)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    northNearbyTileGroups.Add(nearbyGroup);
                    northTileGroups.Add(roomGroup);
                }
            }
            for (int i = 0; i < northNearbyTileGroups.Count; i++)
            {
                if (northNearbyTileGroups[i].Count > 3)
                {
                    var middle = northNearbyTileGroups[i].Count / 2;
                    northNearbyTileGroups[i][middle].Type = TileType.Floor;
                    northNearbyTileGroups[i][middle - 1].Type = TileType.Floor;
                    northTileGroups[i][middle].Type = TileType.Door;
                    northTileGroups[i][middle - 1].Type = TileType.Door;
                }
            }

            // South wall
            tempNearbyTiles = new List<TileData>();
            tempRoomTiles = new List<TileData>();
            nearbyGroup = new List<TileData>();
            roomGroup = new List<TileData>();
            var southNearbyTileGroups = new List<List<TileData>>();
            var southTileGroups = new List<List<TileData>>();
            foreach (var wall in roomWalls.Where(w => w.Y == room.Y_Min).ToList()) // looping through all room walls
            {
                var nearbyTile = hallwayTiles.FirstOrDefault(t => t.X == wall.X && t.Y == wall.Y - 1);
                if (nearbyTile != null) // if tile exists
                {
                    tempNearbyTiles.Add(nearbyTile);
                    tempRoomTiles.Add(wall);
                }
            }
            for (int i = 0; i < tempNearbyTiles.Count - 1; i++)
            {
                var isLast = false;
                nearbyGroup.Add(tempNearbyTiles[i]);
                roomGroup.Add(tempRoomTiles[i]);
                if (tempNearbyTiles[i].Y != tempNearbyTiles[i + 1].Y || Mathf.Abs(tempNearbyTiles[i].X - tempNearbyTiles[i + 1].X) > 1) // if tile out of range
                {
                    // create new group
                    southNearbyTileGroups.Add(nearbyGroup);
                    southTileGroups.Add(roomGroup);
                    nearbyGroup = new List<TileData>();
                    roomGroup = new List<TileData>();
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    i++;
                    if (i == tempNearbyTiles.Count - 1)
                    {
                        isLast = true;
                    }
                }
                else if (i == tempNearbyTiles.Count - 2)
                {
                    isLast = true;

                }
                if (isLast)
                {
                    nearbyGroup.Add(tempNearbyTiles[i + 1]);
                    roomGroup.Add(tempRoomTiles[i + 1]);
                    southNearbyTileGroups.Add(nearbyGroup);
                    southTileGroups.Add(roomGroup);
                }
            }
            for (int i = 0; i < southNearbyTileGroups.Count; i++)
            {
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
    }

    public void OverrideTile(TileData tile, string type)
    {
        floorTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), tileDict[type]);
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

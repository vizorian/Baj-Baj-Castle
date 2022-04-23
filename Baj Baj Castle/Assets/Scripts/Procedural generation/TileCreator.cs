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

    public void FindNeighbours(List<Room> rooms)
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
        SetTiles(doorTiles);
        Debug.Log("floors");
        SetTiles(floorTiles);
        Debug.Log("walls");
        SetTiles(wallTiles, floorTiles);

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
                // if tile is at door position
                if (tile.Type != TileType.None)
                {
                    continue;
                }

                // If there's no tile diagonally adjacent to this one, it's a wall
                if (!room.Tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y + 1)
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

        //print door count
        Debug.Log("Door count: " + rooms.SelectMany(x => x.Tiles).Where(x => x.Type == TileType.Door).Count());
        //print wall count
        Debug.Log("Wall count: " + rooms.SelectMany(x => x.Tiles).Where(x => x.Type == TileType.Wall).Count());

        foreach (var tile in hallwayTiles)
        {
            // If there's no tile diagonally adjacent to this one, it's a wall
            if (!hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y + 1)
                || !hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y + 1)
                || !hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y - 1)
                || !hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y - 1))
            {
                // If there's a room tile of type Door in any direction, it's a floor
                if (rooms.Any(r => r.Tiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y && t.Type == TileType.Door)
                                || r.Tiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y && t.Type == TileType.Door)
                                || r.Tiles.Any(t => t.X == tile.X && t.Y == tile.Y - 1 && t.Type == TileType.Door)
                                || r.Tiles.Any(t => t.X == tile.X && t.Y == tile.Y + 1 && t.Type == TileType.Door)))
                {
                    tile.Type = TileType.Floor;
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

    private void SetTiles(List<TileData> tiles, List<TileData> tilesToCheck = null)
    {
        if (tilesToCheck == null)
        {
            tilesToCheck = tiles;
        }

        // var floorTiles = tiles.Where(x => x.Type == TileType.Floor).ToList();
        // var wallTiles = tiles.Where(x => x.Type == TileType.Wall).ToList();
        // var doorTiles = tiles.Where(x => x.Type == TileType.Door).ToList();
        // var copies = new TileData[floorTiles.Count + wallTiles.Count];
        // floorTiles.CopyTo(copies);
        // wallTiles.CopyTo(copies, floorTiles.Count);
        // var allTiles = new List<TileData>(copies);

        foreach (var tile in tiles)
        {
            var location = new Vector3Int(tile.X, tile.Y, 0);
            TileType tileType = TileType.None;
            string tileName = "";
            switch (tile.Type)
            {
                case TileType.Wall:
                    tileType = TileType.Wall;
                    tileName += "Wall";
                    tilesToCheck = tiles;
                    collisionTilemap.SetTile(location, tileDict["Collision"]);
                    break;
                case TileType.Floor:
                    tileType = TileType.Floor;
                    tileName += "Floor";
                    tilesToCheck = tiles;
                    break;
                case TileType.Door:
                    tileType = TileType.Door;
                    tileName += "Door";
                    floorTilemap.SetTile(location, tileDict[tileName]);
                    continue;
            }

            // check all directions for any tile
            var hasWest = tilesToCheck.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
            var hasEast = tilesToCheck.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
            var hasNorth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
            var hasSouth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y - 1);

            tileName += ProcessTile(hasWest, hasEast, hasNorth, hasSouth, tileType);
            floorTilemap.SetTile(location, tileDict[tileName]);
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

        switch (tileType)
        {
            case TileType.Wall:
                if (!W && !E && N && S)
                {

                }
                break;
            case TileType.Floor:
                break;
            default:
                Debug.Log("ERROR" + tileType);
                break;
        }

        return "";
    }

    public void CreateRoomTiles(List<Room> rooms)
    {
        for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++) // iterate through rooms
        {
            var room = rooms[roomIndex];
            Debug.Log("Filling room " + roomIndex);

            // Separate room tiles into floors and walls
            foreach (var tile in room.Tiles)
            {

            }

            var wallTiles = room.Tiles.Where(x => x.Type == TileType.Wall).ToList();
            var floorTiles = room.Tiles.Where(x => x.Type == TileType.Floor).ToList();

            foreach (var tile in room.Tiles)
            {
                List<TileData> tilesToCheck = new List<TileData>();
                var location = new Vector3Int(tile.X, tile.Y, 0);

                if (collisionTilemap == null)
                {
                    Debug.Log("Collision tilemap is null");
                }

                if (location == null)
                {
                    Debug.Log("Location is null");
                }

                if (tileDict == null)
                {
                    Debug.Log("Tiles is null");
                }

                Debug.Log("Tiles count: " + tileDict.Count);

                string tileName = "";
                switch (tile.Type)
                {
                    case TileType.Wall:
                        tileName += "Wall";
                        tilesToCheck = room.Tiles;
                        collisionTilemap.SetTile(location, tileDict["Collision"]);
                        break;
                    case TileType.Floor:
                        tileName += "Floor";
                        tilesToCheck = floorTiles;
                        break;
                }

                // check all directions for any tile
                var hasWest = tilesToCheck.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
                var hasEast = tilesToCheck.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
                var hasNorth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
                var hasSouth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y - 1);

                if (!hasWest && hasEast && hasNorth && hasSouth)
                {
                    tileName += "W";
                }
                else if (hasWest && !hasEast && hasNorth && hasSouth)
                {
                    tileName += "E";
                }
                else if (hasWest && hasEast && !hasNorth && hasSouth)
                {
                    tileName += "N";
                }
                else if (hasWest && hasEast && hasNorth && !hasSouth)
                {
                    tileName += "S";
                }
                else if (!hasWest && hasEast && !hasNorth && hasSouth)
                {
                    tileName += "NW";
                }
                else if (hasWest && !hasEast && !hasNorth && hasSouth)
                {
                    tileName += "NE";
                }
                else if (!hasWest && hasEast && hasNorth && !hasSouth)
                {
                    tileName += "SW";
                }
                else if (hasWest && !hasEast && hasNorth && !hasSouth)
                {
                    tileName += "SE";
                }

                floorTilemap.SetTile(location, tileDict[tileName]);
            }
        }
    }

    public void CreateHallwayTiles(List<TileData> hallwayTiles)
    {
        foreach (var tile in hallwayTiles)
        {
            var location = new Vector3Int(tile.X, tile.Y, 0);

            // for finding floors
            var hasNorthWest = hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y + 1);
            var hasNorthEast = hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y + 1);
            var hasSouthWest = hallwayTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y - 1);
            var hasSouthEast = hallwayTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y - 1);


            // flawed logic as walls can be surrounded
            if (hasNorthWest || hasNorthEast || hasSouthWest || hasSouthEast)
            {
                tile.Type = TileType.Wall;
            }
            else
            {
                tile.Type = TileType.Floor;
            }
        }

        var wallTiles = hallwayTiles.Where(x => x.Type == TileType.Wall).ToList();
        var floorTiles = hallwayTiles.Where(x => x.Type == TileType.Floor).ToList();

        foreach (var tile in hallwayTiles)
        {
            List<TileData> tilesToCheck = new List<TileData>();
            var location = new Vector3Int(tile.X, tile.Y, 0);

            string tileName = "";
            switch (tile.Type)
            {
                case TileType.Wall:
                    tileName += "Wall";
                    tilesToCheck = hallwayTiles;
                    collisionTilemap.SetTile(location, tileDict["Collision"]);
                    break;
                case TileType.Floor:
                    tileName += "Floor";
                    tilesToCheck = hallwayTiles;
                    break;
            }

            // check all directions for any tile
            var hasWest = tilesToCheck.Any(t => t.X == tile.X - 1 && t.Y == tile.Y);
            var hasEast = tilesToCheck.Any(t => t.X == tile.X + 1 && t.Y == tile.Y);
            var hasNorth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y + 1);
            var hasSouth = tilesToCheck.Any(t => t.X == tile.X && t.Y == tile.Y - 1);

            if (!hasWest && hasEast && hasNorth && hasSouth)
            {
                tileName += "W";
            }
            else if (hasWest && !hasEast && hasNorth && hasSouth)
            {
                tileName += "E";
            }
            else if (hasWest && hasEast && !hasNorth && hasSouth)
            {
                tileName += "N";
            }
            else if (hasWest && hasEast && hasNorth && !hasSouth)
            {
                tileName += "S";
            }
            else if (!hasWest && hasEast && !hasNorth && hasSouth)
            {
                tileName += "NW";
            }
            else if (hasWest && !hasEast && !hasNorth && hasSouth)
            {
                tileName += "NE";
            }
            else if (!hasWest && hasEast && hasNorth && !hasSouth)
            {
                tileName += "SW";
            }
            else if (hasWest && !hasEast && hasNorth && !hasSouth)
            {
                tileName += "SE";
            }

            floorTilemap.SetTile(location, tileDict[tileName]);
        }
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

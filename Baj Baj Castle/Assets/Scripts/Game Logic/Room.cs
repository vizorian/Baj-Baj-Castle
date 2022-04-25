using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    // game logic
    public bool IsActive = false; // activates actors inside the room
    public bool IsCleared = false; // if all actors are dead

    // stuff inside room
    public List<Actor> Actors = new List<Actor>();
    public List<Item> Items = new List<Item>();
    public List<Interactable> Objects = new List<Interactable>();
    public int Id;
    public List<Room> Neighbours = new List<Room>();
    public List<Room> JointRooms = new List<Room>();
    public List<TileData> Tiles;
    public List<Vector2> DoorPositions = new List<Vector2>();
    public int X_Max;
    public int X_Min;
    public int Y_Max;
    public int Y_Min;
    public TileData Center;
    public int Width { get { return Mathf.Abs(X_Max - X_Min - 2); } }
    public int Height { get { return Mathf.Abs(Y_Max - Y_Min - 2); } }
    public int Area { get { return Width * Height; } }
    public float WidthToWorld { get { return (Width + 1) * LevelGenerator.CELL_SIZE; } }
    public float HeightToWorld { get { return (Height + 1) * LevelGenerator.CELL_SIZE; } }
    // TODO get this as float?
    public Vector3 CenterPosition { get { return new Vector3((X_Max + X_Min) / 2 * LevelGenerator.CELL_SIZE + LevelGenerator.CELL_SIZE / 2, (Y_Max + Y_Min) / 2 * LevelGenerator.CELL_SIZE + LevelGenerator.CELL_SIZE / 2); } }
    public Vector3 TopLeftCorner { get { return new Vector3(X_Min * LevelGenerator.CELL_SIZE + LevelGenerator.CELL_SIZE, Y_Max * LevelGenerator.CELL_SIZE - LevelGenerator.CELL_SIZE); } }
    public Room(int id, List<TileData> tiles)
    {
        Id = id;
        Tiles = tiles;
        X_Max = Tiles.Max(x => x.X);
        Y_Max = Tiles.Max(x => x.Y);
        X_Min = Tiles.Min(x => x.X);
        Y_Min = Tiles.Min(x => x.Y);
        Center = Tiles.First(x => x.X == (X_Max + X_Min) / 2 && x.Y == (Y_Max + Y_Min) / 2);
    }

    public void SharesWall(Room otherRoom)
    {
        if (Neighbours.Contains(otherRoom) || otherRoom.Neighbours.Contains(this))
        {
            return;
        }
        bool sharesWall = false;
        var matchingTiles = new List<TileData>();
        foreach (var tile in Tiles)
        {
            if (otherRoom.Tiles.Contains(tile))
            {
                if (!sharesWall)
                {
                    Neighbours.Add(otherRoom);
                    otherRoom.Neighbours.Add(this);
                    sharesWall = true;
                }
                matchingTiles.Add(tile);
            }
        }

        if (!sharesWall)
        {
            return;
        }

        TileData first = null;
        TileData second = null;

        if (matchingTiles.Count > 3)
        {
            var middle = matchingTiles.Count / 2;
            first = matchingTiles[middle];
            second = matchingTiles[middle - 1];
            DoorPositions.Add(new Vector2(first.X, first.Y));
            DoorPositions.Add(new Vector2(second.X, second.Y));
        }

        foreach (var tile in matchingTiles)
        {
            if (first != null && second != null)
            {
                if (tile == first || tile == second)
                {
                    otherRoom.Tiles.Remove(tile);
                    Tiles.Remove(tile);
                    tile.Type = TileType.Door;
                    otherRoom.Tiles.Add(tile);
                    Tiles.Add(tile);
                }
            }
        }
    }
    public void WallNearby(Room otherRoom)
    {
        if (JointRooms.Contains(otherRoom) || otherRoom.JointRooms.Contains(this))
        {
            return;
        }

        TileData closeTile = null;
        var otherRoomOuterTiles = otherRoom.Tiles.Where(t => t.X == otherRoom.X_Min || t.X == otherRoom.X_Max || t.Y == otherRoom.Y_Max || t.Y == otherRoom.Y_Min).ToList();
        foreach (var tile in Tiles)
        {
            if (otherRoomOuterTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y))
            {
                closeTile = new TileData(tile.X + 1, tile.Y, TileType.None);
                break;
            }
            else if (otherRoomOuterTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y))
            {
                closeTile = new TileData(tile.X - 1, tile.Y, TileType.None);
                break;
            }
            else if (otherRoomOuterTiles.Any(t => t.X == tile.X && t.Y == tile.Y + 1))
            {
                closeTile = new TileData(tile.X, tile.Y + 1, TileType.None);
                break;
            }
            else if (otherRoomOuterTiles.Any(t => t.X == tile.X && t.Y == tile.Y - 1))
            {
                closeTile = new TileData(tile.X, tile.Y - 1, TileType.None);
                break;
            }
        }

        if (closeTile != null)
        {
            JointRooms.Add(otherRoom);
            otherRoom.JointRooms.Add(this);

            if (closeTile.X > X_Max)
            {
                X_Max++;
                for (int i = Y_Min; i <= Y_Max; i++)
                {
                    Tiles.Add(new TileData(X_Max, i, TileType.None));
                }
            }
            else if (closeTile.X < X_Min)
            {
                X_Min--;
                for (int i = Y_Min; i <= Y_Max; i++)
                {
                    Tiles.Add(new TileData(X_Min, i, TileType.None));
                }
            }
            else if (closeTile.Y > Y_Max)
            {
                Y_Max++;
                for (int i = X_Min; i <= X_Max; i++)
                {
                    Tiles.Add(new TileData(i, Y_Max, TileType.None));
                }
            }
            else if (closeTile.Y < Y_Min)
            {
                Y_Min--;
                for (int i = X_Min; i <= X_Max; i++)
                {
                    Tiles.Add(new TileData(i, Y_Min, TileType.None));
                }
            }
            else
            {
                Debug.Log("ERROR TILE at CLOSE TILE");
            }
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        Room other = (Room)obj;
        return this.Id == other.Id;
    }

    public Vector3 GetRandomTile()
    {
        Vector3 position = new Vector3();
        var offset = LevelGenerator.CELL_SIZE / 2;
        var tiles = Tiles.Where(t => t.Type != TileType.Wall && t.Type != TileType.Door).ToList();
        do
        {
            var randomTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
            position = new Vector3(randomTile.X * LevelGenerator.CELL_SIZE + offset, randomTile.Y * LevelGenerator.CELL_SIZE + offset);
        } // check if list of Actors or Objects contains anything at this position
        while (Actors.Any(a => a.transform.position == position) || Objects.Any(o => o.transform.position == position));

        Debug.Log("Random tile: " + position);
        return position;
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

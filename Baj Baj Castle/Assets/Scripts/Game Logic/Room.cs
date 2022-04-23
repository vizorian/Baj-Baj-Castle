using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public int Id;
    public bool IsExplored = false;
    public List<Room> Neighbours = new List<Room>();
    public List<Room> JointRooms = new List<Room>();
    public List<TileData> Tiles;
    public List<Vector2> DoorPositions = new List<Vector2>();
    public int X_Max;
    public int X_Min;
    public int Y_Max;
    public int Y_Min;

    public Room(int id, List<TileData> tiles)
    {
        Id = id;
        Tiles = tiles;
        X_Max = Tiles.Max(x => x.X);
        Y_Max = Tiles.Max(x => x.Y);
        X_Min = Tiles.Min(x => x.X);
        Y_Min = Tiles.Min(x => x.Y);
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

    public override int GetHashCode()
    {
        return Id;
    }
}

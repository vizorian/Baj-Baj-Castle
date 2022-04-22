using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public bool IsExplored = false;
    public List<Room> Neighbours = new List<Room>();
    public List<TileData> Tiles;
    // TODO PRIORITY get door positions from hallways as ints
    // to do that, need to check if the hallway point is vertical or horizontal
    // and then get the 2 rounded values.
    public List<Vector2> DoorPositions = new List<Vector2>();
    public int X_Max;
    public int X_Min;
    public int Y_Max;
    public int Y_Min;

    public Room()
    {
        Tiles = new List<TileData>();
    }

    public Room(List<TileData> tiles)
    {
        Tiles = tiles;
        X_Max = Tiles.Max(x => x.X);
        Y_Max = Tiles.Max(x => x.Y);
        X_Min = Tiles.Min(x => x.X);
        Y_Min = Tiles.Min(x => x.Y);
    }

    public void SharesWall(Room room)
    {
        if (Neighbours.Contains(room))
        {
            return;
        }
        bool sharesWall = false;
        var tilesToAdd = new List<TileData>();
        foreach (var tile in Tiles)
        {
            if (room.Tiles.Contains(tile))
            {
                if (!sharesWall)
                {
                    Neighbours.Add(room);
                    room.Neighbours.Add(this);
                    sharesWall = true;
                }
                tilesToAdd.Add(tile);
            }
        }
        Tiles.AddRange(tilesToAdd);
    }
}

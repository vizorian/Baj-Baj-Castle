using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public bool IsExplored = false;
    public List<TileData> Tiles;
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
}

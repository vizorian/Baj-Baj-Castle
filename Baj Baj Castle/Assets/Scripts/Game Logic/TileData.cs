using System.Collections;
using System.Collections.Generic;

// TODO potentially include neighbour bools
public class TileData
{
    public int X;
    public int Y;
    public TileType Type;

    public TileData(int x, int y, TileType type)
    {
        X = x;
        Y = y;
        Type = type;
    }

    public TileData()
    {
    }

    public override string ToString()
    {
        return $"{X}, {Y}, {Type}";
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

        TileData other = (TileData)obj;

        return X == other.X && Y == other.Y && Type == other.Type;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

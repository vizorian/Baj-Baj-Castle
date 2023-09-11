namespace Game_Logic.Tiles;

public class TileObjectData
{
    public TileType Type;
    public readonly int X;
    public readonly int Y;

    public TileObjectData(int x, int y, TileType type)
    {
        X = x;
        Y = y;
        Type = type;
    }

    public override string ToString()
    {
        return $"{X}, {Y}, {Type}";
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        if (obj.GetType() != GetType()) return false;

        var other = (TileObjectData)obj;

        return X == other.X && Y == other.Y && Type == other.Type;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }
}
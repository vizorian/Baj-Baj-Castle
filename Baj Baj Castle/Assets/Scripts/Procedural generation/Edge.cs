namespace Procedural_generation;

public class Edge
{
    public readonly Point P1;
    public readonly Point P2;

    public Edge(Point p1, Point p2)
    {
        P1 = p1;
        P2 = p2;
    }

    public float Weight => P1.DistanceTo(P2);

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        var edge = (Edge)obj;
        var identical = P1.Equals(edge.P1) && P2.Equals(edge.P2);
        var identicalReverse = P1.Equals(edge.P2) && P2.Equals(edge.P1);

        return identical || identicalReverse;
    }

    public override int GetHashCode()
    {
        var hash = (int)P1.X ^ (int)P1.Y ^ (int)P2.X ^ (int)P2.Y;
        return hash.GetHashCode();
    }
}
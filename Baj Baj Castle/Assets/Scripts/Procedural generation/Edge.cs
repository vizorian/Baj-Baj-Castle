using System;
using UnityEngine;

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

    public bool Overlaps(Cell cell)
    {
        var cellX = cell.SimulationCell.transform.position.x;
        var cellY = cell.SimulationCell.transform.position.y;
        var cellWidth = cell.SimulationCell.transform.localScale.x;
        var cellHeight = cell.SimulationCell.transform.localScale.y;

        var p1X = (float) P1.X;
        var p1Y = (float) P1.Y;
        var p2X = (float) P2.X;
        var p2Y = (float) P2.Y;

        var minX = Mathf.Min(p1X, p2X);
        var maxX = Mathf.Max(p1X, p2X);
        var minY = Mathf.Min(p1Y, p2Y);
        var maxY = Mathf.Max(p1Y, p2Y);

        var cellMinX = cellX - cellWidth / 2;
        var cellMaxX = cellX + cellWidth / 2;
        var cellMinY = cellY - cellHeight / 2;
        var cellMaxY = cellY + cellHeight / 2;

        return minX <= cellMaxX && maxX >= cellMinX && minY <= cellMaxY && maxY >= cellMinY;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        var edge = (Edge) obj;
        var identical = P1.Equals(edge.P1) && P2.Equals(edge.P2);
        var identicalReverse = P1.Equals(edge.P2) && P2.Equals(edge.P1);

        return identical || identicalReverse;
    }

    public override int GetHashCode()
    {
        var hash = (int) P1.X ^ (int) P1.Y ^ (int) P2.X ^ (int) P2.Y;
        return hash.GetHashCode();
    }

    public bool SharesPoint(Edge otherEdge)
    {
        return P1.Equals(otherEdge.P1) || P1.Equals(otherEdge.P2) || P2.Equals(otherEdge.P1) || P2.Equals(otherEdge.P2);
    }

    public Point[] GetNonSharedPoints(Edge otherEdge)
    {
        if (P1.Equals(otherEdge.P1))
            return new[] {P2, otherEdge.P2};
        if (P1.Equals(otherEdge.P2))
            return new[] {P2, otherEdge.P1};
        if (P2.Equals(otherEdge.P1))
            return new[] {P1, otherEdge.P2};
        if (P2.Equals(otherEdge.P2))
            return new[] {P1, otherEdge.P1};
        return Array.Empty<Point>();
    }
}
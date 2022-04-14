using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Point P1;
    public Point P2;
    public float Weight
    {
        get{ return P1.DistanceTo(P2); }
    }
    public Edge(Point p1, Point p2)
    {
        P1 = p1;
        P2 = p2;
    }

    public bool Overlaps(Cell cell){
        var cellX = cell.SimulationCell.transform.position.x;
        var cellY = cell.SimulationCell.transform.position.y;
        var cellWidth = cell.SimulationCell.transform.localScale.x;
        var cellHeight = cell.SimulationCell.transform.localScale.y;

        var p1X = (float)P1.X;
        var p1Y = (float)P1.Y;
        var p2X = (float)P2.X;
        var p2Y = (float)P2.Y;

        var minX = Mathf.Min(p1X, p2X);
        var maxX = Mathf.Max(p1X, p2X);
        var minY = Mathf.Min(p1Y, p2Y);
        var maxY = Mathf.Max(p1Y, p2Y);

        var cellMinX = cellX - cellWidth / 2;
        var cellMaxX = cellX + cellWidth / 2;
        var cellMinY = cellY - cellHeight / 2;
        var cellMaxY = cellY + cellHeight / 2;

        return (minX <= cellMaxX && maxX >= cellMinX && minY <= cellMaxY && maxY >= cellMinY);
    }

    public bool SharesPoint(Edge other)
    {
        // convert points to int
        var p1X = (int)P1.X;
        var p1Y = (int)P1.Y;
        var p2X = (int)P2.X;
        var p2Y = (int)P2.Y;

        var otherP1X = (int)other.P1.X;
        var otherP1Y = (int)other.P1.Y;
        var otherP2X = (int)other.P2.X;
        var otherP2Y = (int)other.P2.Y;

        // check if points are the same
        if (p1X == otherP1X && p1Y == otherP1Y)
        {
            return true;
        }
        if (p1X == otherP2X && p1Y == otherP2Y)
        {
            return true;
        }
        if (p2X == otherP1X && p2Y == otherP1Y)
        {
            return true;
        }
        if (p2X == otherP2X && p2Y == otherP2Y)
        {
            return true;
        }
        return false;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        Edge edge = (Edge)obj;
        bool identical = P1.Equals(edge.P1) && P2.Equals(edge.P2);
        bool identicalReverse = P1.Equals(edge.P2) && P2.Equals(edge.P1);

        return identical || identicalReverse;
    }

    public override int GetHashCode()
    {
        int hash = (int)P1.X ^ (int)P1.Y ^ (int)P2.X ^ (int)P2.Y;
        return hash.GetHashCode();
    }
}

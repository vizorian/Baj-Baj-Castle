using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Point P1;
    public Point P2;

    public Edge(Point p1, Point p2)
    {
        P1 = p1;
        P2 = p2;
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

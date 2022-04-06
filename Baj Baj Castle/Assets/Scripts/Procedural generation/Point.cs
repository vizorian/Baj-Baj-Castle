using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public double X;
    public double Y;
    public HashSet<Triangle> AdjacentTriangles = new HashSet<Triangle>();

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    // Check if points is within triangle via barycentric coordinates
    public bool IsWithinTriangle(Triangle triangle)
    {
        var a = triangle.Vertices[0];
        var b = triangle.Vertices[1];
        var c = triangle.Vertices[2];

        var denominator  = ((b.Y - c.Y)*(a.X - c.X) + (c.X - b.X)*(a.Y - c.Y));
        var resA = ((b.Y - c.Y)*(X - c.X) + (c.X - b.X)*(Y - c.Y)) / denominator;
        var resB = ((c.Y - a.Y)*(X - c.X) + (a.X - c.X)*(Y - c.Y)) / denominator;
        var resC = 1 - resA - resB;

        return 0 <= resA && resA <= 1 && 0 <= resB && resB <= 1 && 0 <= resC && resC <= 1;
    }

    // Return distance to other point
    public float DistanceTo(Point point){
        return Mathf.Sqrt((Mathf.Pow((float)(X - point.X), 2) + Mathf.Pow((float)(Y - point.Y), 2)));
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Point point = (Point)obj;
        return X == point.X && Y == point.Y;
    }

    public override int GetHashCode()
    {
        int hash = (int)X ^ (int)Y;
        return hash.GetHashCode();
    }
}

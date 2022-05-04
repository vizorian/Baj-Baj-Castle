using System;
using UnityEngine;

public class Point
{
    public double X;
    public double Y;

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    // Return distance to other point
    public float DistanceTo(Point point)
    {
        return Mathf.Sqrt(Mathf.Pow((float)(X - point.X), 2) + Mathf.Pow((float)(Y - point.Y), 2));
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        var point = (Point)obj;
        var TOLERANCE = 0.001;
        return Math.Abs(X - point.X) < TOLERANCE && Math.Abs(Y - point.Y) < TOLERANCE;
    }

    public override int GetHashCode()
    {
        var hash = (int)X ^ (int)Y;
        return hash.GetHashCode();
    }

    public override string ToString()
    {
        return "(" + X + ", " + Y + ")";
    }
}
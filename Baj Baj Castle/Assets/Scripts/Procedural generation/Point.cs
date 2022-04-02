using System.Collections.Generic;

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
}

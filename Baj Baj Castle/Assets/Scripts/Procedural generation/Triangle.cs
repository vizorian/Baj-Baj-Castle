using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Triangle
{
    public Point[] Vertices = new Point[3];
    public Point Circumcenter;
    public double RadiusSq;

    public HashSet<Triangle> TrianglesBordering
    {
        get
        {
            var neighbours = new HashSet<Triangle>();
            foreach (Point vertex in Vertices)
            {
                var trianglesBordering = vertex.AdjacentTriangles.Where(t =>
                {
                    return t != this && BordersWith(t);
                });
                neighbours.UnionWith(trianglesBordering);
            }
            return neighbours;
        }
    }

    public Triangle(Point p1, Point p2, Point p3)
    {
        Vertices[0] = p1;
        if(IsCounterClockwise(p1, p2, p3))
        {
            Vertices[1] = p2;
            Vertices[2] = p3;
        }
        else
        {
            Vertices[1] = p3;
            Vertices[2] = p2;
        }

        Vertices[0].AdjacentTriangles.Add(this);
        Vertices[1].AdjacentTriangles.Add(this);
        Vertices[2].AdjacentTriangles.Add(this);

        UpdateCircumcircle();
    }

    private void UpdateCircumcircle()
    {
        Point p1 = Vertices[0];
        Point p2 = Vertices[1];
        Point p3 = Vertices[2];

        double dA = p1.X * p1.X + p1.Y * p1.Y;
        double dB = p2.X * p2.X + p2.Y * p2.Y;
        double dC = p3.X * p3.X + p3.Y * p3.Y;

        double aux1 = dA * (p3.Y - p2.Y) + dB * (p1.Y - p3.Y) + dC * (p2.Y - p1.Y);
        double aux2 = -(dA * (p3.X - p2.X) + dB * (p1.X - p3.X) + dC * (p2.X - p1.X));
        double div = 2 * (p1.X * (p3.Y - p2.Y) + p2.X * (p1.Y - p3.Y) + p3.X * (p2.Y - p1.Y));

        Circumcenter = new Point(aux1 / div, aux2 / div);
        RadiusSq = Mathf.Pow((float)(Circumcenter.X - p1.X), 2) + Mathf.Pow((float)(Circumcenter.Y - p1.Y), 2);
    }

    private bool IsCounterClockwise(Point p1, Point p2, Point p3)
    {
        return ((p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y)) > 0;
    }

    public bool BordersWith(Triangle triangle)
    {
        return Vertices.Where(v => triangle.Vertices.Contains(v)).Count() == 2;
    }

    public bool IsWithinCircumcircle(Point p)
    {
        return Mathf.Pow((float)(p.X - Circumcenter.X), 2) + Mathf.Pow((float)(p.Y - Circumcenter.Y), 2) < RadiusSq;
    }
}

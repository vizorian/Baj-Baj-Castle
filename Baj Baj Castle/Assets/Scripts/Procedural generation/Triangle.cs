public class Triangle
{
    public Point Circumcenter;
    public double RadiusSq;
    public readonly Point[] Vertices = new Point[3];

    public Triangle(Point p1, Point p2, Point p3)
    {
        Vertices[0] = p1;
        Vertices[1] = p2;
        Vertices[2] = p3;

        UpdateCircumcircle();
    }

    // Calculate the circumcircle of the triangle
    private void UpdateCircumcircle()
    {
        var p1 = Vertices[0];
        var p2 = Vertices[1];
        var p3 = Vertices[2];

        var dA = p1.X * p1.X + p1.Y * p1.Y;
        var dB = p2.X * p2.X + p2.Y * p2.Y;
        var dC = p3.X * p3.X + p3.Y * p3.Y;

        var aux1 = dA * (p3.Y - p2.Y) + dB * (p1.Y - p3.Y) + dC * (p2.Y - p1.Y);
        var aux2 = -(dA * (p3.X - p2.X) + dB * (p1.X - p3.X) + dC * (p2.X - p1.X));
        var div = 2 * (p1.X * (p3.Y - p2.Y) + p2.X * (p1.Y - p3.Y) + p3.X * (p2.Y - p1.Y));

        Circumcenter = new Point(aux1 / div, aux2 / div);
        RadiusSq = (Circumcenter.X - p1.X) * (Circumcenter.X - p1.X) +
                   (Circumcenter.Y - p1.Y) * (Circumcenter.Y - p1.Y);
    }

    // Check if triangle contains edge
    public bool ContainsEdge(Edge edge)
    {
        return edge.Equals(new Edge(Vertices[0], Vertices[1])) ||
               edge.Equals(new Edge(Vertices[1], Vertices[2])) ||
               edge.Equals(new Edge(Vertices[2], Vertices[0]));
    }

    // Check if point is within triangle circumcircle
    public bool IsWithinCircumcircle(Point p)
    {
        var distSq = (p.X - Circumcenter.X) * (p.X - Circumcenter.X) + (p.Y - Circumcenter.Y) * (p.Y - Circumcenter.Y);
        return distSq < RadiusSq;
    }

    public override string ToString()
    {
        return string.Format("A {0} {1}\nB {2} {3}\nC {4} {5}\n", Vertices[0].X, Vertices[0].Y, Vertices[1].X,
            Vertices[1].Y, Vertices[2].X, Vertices[2].Y);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        var triangle = (Triangle) obj;
        return Vertices[0].Equals(triangle.Vertices[0]) &&
               Vertices[1].Equals(triangle.Vertices[1]) &&
               Vertices[2].Equals(triangle.Vertices[2]);
    }

    public override int GetHashCode()
    {
        var hash = (int) Vertices[0].X ^ (int) Vertices[0].Y ^ (int) Vertices[1].X ^ (int) Vertices[1].Y ^
                   (int) Vertices[2].X ^ (int) Vertices[2].Y;
        return hash.GetHashCode();
    }
}
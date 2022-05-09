using System.Collections.Generic;
using System.Linq;

public class DelaunayTriangulator
{
    // Do bowyer watson incremental triangulation
    public HashSet<Triangle> BowyerWatson(HashSet<Point> points)
    {
        // Create super triangle
        var supra = CreateSupraTriangle(points);
        var triangulation = new HashSet<Triangle> { supra };

        foreach (var point in points)
        {
            // Find invalid triangles due to insertion
            var badTriangles = new HashSet<Triangle>();
            foreach (var triangle in triangulation)
                if (triangle.IsWithinCircumcircle(point))
                    badTriangles.Add(triangle);

            // Find the boundary of the polygonal hole
            var polygon = new HashSet<Edge>();
            foreach (var triangle in badTriangles)
            {
                var otherTriangles = badTriangles.Where(t => !Equals(t, triangle));
                for (var i = 0; i < 3; i++)
                {
                    var edge = new Edge(triangle.Vertices[i], triangle.Vertices[(i + 1) % 3]);
                    if (!otherTriangles.Any(t => t.ContainsEdge(edge))) polygon.Add(edge);
                }
            }

            // Remove bad triangles from data structure
            foreach (var triangle in badTriangles) triangulation.Remove(triangle);

            // Retriangulate the polygonal hole
            foreach (var edge in polygon)
            {
                var newTriangle = new Triangle(edge.P1, edge.P2, point);
                triangulation.Add(newTriangle);
            }
        }

        // Remove triangles that contain any Supra vertices
        triangulation.RemoveWhere(t => t.Vertices.Contains(supra.Vertices[0])
                                       || t.Vertices.Contains(supra.Vertices[1])
                                       || t.Vertices.Contains(supra.Vertices[2]));

        return triangulation;
    }

    // Create triangle large enough to contain all points
    private static Triangle CreateSupraTriangle(HashSet<Point> points)
    {
        var minX = (float)points.Min(p => p.X) - 20;
        var minY = (float)points.Min(p => p.Y) - 10;
        var maxX = (float)points.Max(p => p.X) + 20;
        var maxY = (float)points.Max(p => p.Y) + 20;

        return new Triangle(new Point(minX, minY), new Point(maxX, minY), new Point((maxX + minX) / 2, maxY));
    }
}
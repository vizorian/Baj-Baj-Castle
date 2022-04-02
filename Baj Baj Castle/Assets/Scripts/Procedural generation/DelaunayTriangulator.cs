using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DelaunayTriangulator
{
    private HashSet<Triangle> border;

    public void CalculateBorder(List<Cell> cells)
    {
    }

    public HashSet<Triangle> BowyerWatson(HashSet<Point> points)
    {
        HashSet<Triangle> triangulation = new HashSet<Triangle>();

        foreach (var point in points)
        {
            var badTriangles = FindBadTriangles(point, triangulation);
            var polygon = FindHoleBoundaries(badTriangles);

            foreach (var triangle in badTriangles)
            {
                foreach (var vertex in triangle.Vertices)
                {
                    vertex.AdjacentTriangles.Remove(triangle);
                }
            }
            triangulation.RemoveWhere(t => badTriangles.Contains(t));

            foreach (var edge in polygon.Where(e => e.P1 != point && e.P2 != point))
            {
                triangulation.Add(new Triangle(point, edge.P1, edge.P2));
            }
        }

        return triangulation;
    }

    private List<Edge> FindHoleBoundaries(HashSet<Triangle> badTriangles)
    {
        var edges = new List<Edge>();
        foreach (Triangle triangle in badTriangles)
        {
            edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
            edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
            edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
        }

        return edges.GroupBy(e => e).Where(e => e.Count() == 1).Select(e => e.First()).ToList(); 
    }

    private HashSet<Triangle> FindBadTriangles(Point p, HashSet<Triangle> triangles)
    {
        return new HashSet<Triangle>(triangles.Where(t => t.IsWithinCircumcircle(p)));
    }
}

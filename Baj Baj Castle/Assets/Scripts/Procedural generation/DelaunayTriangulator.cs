using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DelaunayTriangulator
{
    private Triangle Supra;

    public HashSet<Triangle> BowyerWatson(HashSet<Point> points){
        AdjustSupraTriangle(points);

        var triangulation = new HashSet<Triangle>();
        triangulation.Add(Supra);

        foreach (var point in points)
        {
            // Find invalid triangles due to insertion
            var badTriangles = new HashSet<Triangle>();
            foreach (var triangle in triangulation)
            {
                if (triangle.IsWithinCircumcircle(point))
                {
                    badTriangles.Add(triangle);
                }
            }
            
            // Find the boundary of the polygonal hole
            var polygon = new HashSet<Edge>();
            foreach (var triangle in badTriangles)
            {
                var otherTriangles = badTriangles.Where(t => t != triangle);
                for(int i = 0; i < 3; i++)
                {
                    var edge = new Edge(triangle.Vertices[i], triangle.Vertices[(i + 1) % 3]);
                    if (!otherTriangles.Any(t => t.ContainsEdge(edge)))
                    {
                        polygon.Add(edge);
                    }
                }
            }

            // Remove bad triangles from data structure
            foreach (var triangle in badTriangles)
            {
                triangulation.Remove(triangle);
            }

            // Retriangulate the polygonal hole
            foreach (var edge in polygon)
            {
                var newTriangle = new Triangle(edge.P1, edge.P2, point);
                triangulation.Add(newTriangle);
            }
        }

        // Remove triangles that contain any supra vertices
        triangulation.RemoveWhere(t => t.Vertices.Contains(Supra.Vertices[0])
                                       || t.Vertices.Contains(Supra.Vertices[1])
                                       || t.Vertices.Contains(Supra.Vertices[2]));

        return triangulation;
    }

    // Create supra triangle from most spread out points (kind of)
    public void CreateSupraTriangle(List<Cell> cells)
    {
        var leftCell = cells.OrderBy(c => c.SimulationCell.transform.position.x - c.Width * 0.01f / 2)
                                  .ThenBy(c => c.SimulationCell.transform.position.y - c.Height * 0.01f / 2).First();
        var topCell = cells.OrderByDescending(c => c.SimulationCell.transform.position.y + c.Height * 0.01f / 2).First();
        var rightCell = cells.OrderByDescending(c => c.SimulationCell.transform.position.x + c.Width * 0.01f / 2)
                                   .ThenBy(c => c.SimulationCell.transform.position.y - c.Height * 0.01f / 2).First();

        var pointLeft = new Point((float)leftCell.SimulationCell.transform.position.x, (float)leftCell.SimulationCell.transform.position.y);
        var pointTop = new Point((float)topCell.SimulationCell.transform.position.x, (float)topCell.SimulationCell.transform.position.y);
        var pointRight = new Point((float)rightCell.SimulationCell.transform.position.x, (float)rightCell.SimulationCell.transform.position.y);

        Supra = new Triangle(pointLeft, pointTop, pointRight);
    }

    private void AdjustSupraTriangle(HashSet<Point> points)
    {
        var count = 0;
        while(count < 100){
            var pointsOutside = points.Where(p => !p.IsWithinTriangle(Supra));

            if(pointsOutside.Count() == 0) break;

            var furthestPoint = pointsOutside.OrderByDescending(p => p.DistanceTo(Supra.Circumcenter)).First();
            var distance = furthestPoint.DistanceTo(Supra.Circumcenter) * 0.3f;

            Supra.Vertices[0].X -= distance;
            Supra.Vertices[0].Y -= distance;
            Supra.Vertices[1].Y += distance;
            Supra.Vertices[2].X += distance;
            Supra.Vertices[2].Y -= distance;
            count++;
        }
    }
}

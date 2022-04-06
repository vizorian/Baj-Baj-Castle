using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DelaunayTriangulator
{
    private Triangle Supra;

    // TO DO
    /*
    function BowyerWatson (pointList)
        // pointList is a set of coordinates defining the points to be triangulated
        triangulation := empty triangle mesh data structure
        add super-triangle to triangulation // must be large enough to completely contain all the points in pointList
        
        for each point in pointList do // add all the points one at a time to the triangulation
            badTriangles := empty set
            for each triangle in triangulation do // first find all the triangles that are no longer valid due to the insertion
                if point is inside circumcircle of triangle
                    add triangle to badTriangles

            polygon := empty set

            for each triangle in badTriangles do // find the boundary of the polygonal hole
                for each edge in triangle do
                    if edge is not shared by any other triangles in badTriangles
                        add edge to polygon

            for each triangle in badTriangles do // remove them from the data structure
                remove triangle from triangulation
            for each edge in polygon do // re-triangulate the polygonal hole
                newTri := form a triangle from edge to point
                    add newTri to triangulation

        for each triangle in triangulation // done inserting points, now clean up
            if triangle contains a vertex from original super-triangle
                remove triangle from triangulation
    return triangulation
    */
    public HashSet<Triangle> BowyerWatson(HashSet<Point> points){
        DrawTriangle(Supra);
        AdjustSupraTriangle(points);
        DrawTriangle(Supra);
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

    private void DrawTriangle(Triangle triangle)
    {
        var vertices = triangle.Vertices;
        var p1 = vertices[0];
        var p2 = vertices[1];
        var p3 = vertices[2];

        var line = new GameObject($"Triangle {triangle.GetHashCode()}");
        var lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 3;
        lineRenderer.SetPosition(0, new Vector3((float)p1.X, (float)p1.Y, 0));
        lineRenderer.SetPosition(1, new Vector3((float)p2.X, (float)p2.Y, 0));
        lineRenderer.SetPosition(2, new Vector3((float)p3.X, (float)p3.Y, 0));
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    // Create supra triangle from most spread out points (kind of)
    public void CreateSupraTriangle(List<Cell> cells)
    {
        var leftCell = cells.OrderBy(c => c.DisplayCell.transform.position.x - c.Width * 0.01f / 2)
                                  .ThenBy(c => c.DisplayCell.transform.position.y - c.Height * 0.01f / 2).First();
        var topCell = cells.OrderByDescending(c => c.DisplayCell.transform.position.y + c.Height * 0.01f / 2).First();
        var rightCell = cells.OrderByDescending(c => c.DisplayCell.transform.position.x + c.Width * 0.01f / 2)
                                   .ThenBy(c => c.DisplayCell.transform.position.y - c.Height * 0.01f / 2).First();

        var pointLeft = new Point((float)leftCell.DisplayCell.transform.position.x, (float)leftCell.DisplayCell.transform.position.y);
        var pointTop = new Point((float)topCell.DisplayCell.transform.position.x, (float)topCell.DisplayCell.transform.position.y);
        var pointRight = new Point((float)rightCell.DisplayCell.transform.position.x, (float)rightCell.DisplayCell.transform.position.y);

        Supra = new Triangle(pointLeft, pointTop, pointRight);
    }

    private void AdjustSupraTriangle(HashSet<Point> points)
    {
        var count = 0;
        while(true && count < 100){
            var pointsOutside = points.Where(p => !p.IsWithinTriangle(Supra));

            if(pointsOutside.Count() == 0) break;

            var furthestPoint = pointsOutside.OrderByDescending(p => p.DistanceTo(Supra.Circumcenter)).First();
            var distance = furthestPoint.DistanceTo(Supra.Circumcenter) * 0.1f;

            Supra.Vertices[0].X -= distance;
            Supra.Vertices[0].Y -= distance;
            Supra.Vertices[1].Y += distance;
            Supra.Vertices[2].X += distance;
            Supra.Vertices[2].Y -= distance;
            count++;
        }
    }
}

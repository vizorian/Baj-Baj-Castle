using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject PhysicsCell;
    public GameObject SimulationCell;
    public BoxCollider2D DisplayCollider;

    public Vector2 Position = new Vector2 (0, 0);
    public float Width;
    public float Height;

    public Cell(Vector2 position, float width, float height)
    {
        Width = width;
        Height = height;
        Position = position;
    }

    // check if point is inside
    public bool IsPointInside(Vector2 point)
    {
        // get cell position in world
        var worldPosition = SimulationCell.transform.position;
        var worldPositionX = worldPosition.x;
        var worldPositionY = worldPosition.y;

        // get rectangle corners
        var topLeft = new Vector2(worldPositionX - Width * 0.01f / 2, worldPositionY * 0.01f + Height / 2);
        var bottomRight = new Vector2(worldPositionX + Width * 0.01f / 2, worldPositionY - Height * 0.01f / 2);

        return point.x >= topLeft.x && point.x <= bottomRight.x && point.y <= topLeft.y && point.y >= bottomRight.y;

    }

    public bool IsPartOf(HashSet<Triangle> triangles)
    {
        // Get all unique vertices from triangles
        HashSet<Point> vertices = new HashSet<Point>();
        foreach (Triangle triangle in triangles)
        {
            vertices.Add(triangle.Vertices[0]);
            vertices.Add(triangle.Vertices[1]);
            vertices.Add(triangle.Vertices[2]);
        }

        // Check if the cell is any of the vertices
        foreach (Point vertex in vertices)
        {
            if (vertex.X == SimulationCell.transform.position.x && vertex.Y == SimulationCell.transform.position.y)
            {
                return true;
            }
        }
        
        return false;
    }
}

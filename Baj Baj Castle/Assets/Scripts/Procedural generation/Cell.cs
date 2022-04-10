using System;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject PhysicsCell;
    public GameObject SimulationCell;

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
    public bool IsPointInside(Point point)
    {
        // get cell position in world
        var cellX = SimulationCell.transform.position.x;
        var cellY = SimulationCell.transform.position.y;

        var offsetX = SimulationCell.transform.localScale.x / 2;
        var offsetY = SimulationCell.transform.localScale.y / 2;

        // get rectangle corners
        var topLeft = new Point(cellX - offsetX, cellY + offsetY);
        var bottomRight = new Point(cellX + offsetX, cellY - offsetY);

        return point.X > topLeft.X
               && point.X < bottomRight.X
               && point.Y < topLeft.Y
               && point.Y > bottomRight.Y;

    }

    public bool IsOverlapping(Cell other){
        var x = SimulationCell.transform.position.x;
        var y = SimulationCell.transform.position.y;
        var shrinkage = 0.001f;
        // var shrinkage = 0f;

        var offsetX = Width * LevelGenerator.PIXEL_SIZE / 2 - shrinkage;
        var offsetY = Height * LevelGenerator.PIXEL_SIZE / 2 - shrinkage;

        // Get SimulationCell corners
        var topLeft = new Vector2(x - offsetX, y + offsetY);
        var bottomRight = new Vector2(x + offsetX, y - offsetY);

        var otherSimulationCell = other.SimulationCell;

        // Do same for other cell
        var otherX = otherSimulationCell.transform.position.x;
        var otherY = otherSimulationCell.transform.position.y;

        var otherOffsetX = other.Width * LevelGenerator.PIXEL_SIZE / 2 - shrinkage;
        var otherOffsetY = other.Height * LevelGenerator.PIXEL_SIZE / 2 - shrinkage;

        // Get other cell corners
        var otherTopLeft = new Vector2(otherX - otherOffsetX, otherY + otherOffsetY);
        var otherBottomRight = new Vector2(otherX + otherOffsetX, otherY - otherOffsetY);

        if (topLeft.x > otherBottomRight.x
            || otherTopLeft.x > bottomRight.x
            || bottomRight.y > otherTopLeft.y
            || otherBottomRight.y > topLeft.y)
        {
            return false;
        }
        return true;
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

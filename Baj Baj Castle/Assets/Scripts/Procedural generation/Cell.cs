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
    public bool IsPointInside(Vector2 point)
    {
        // get cell position in world
        var worldPosition = SimulationCell.transform.position;
        var worldPositionX = worldPosition.x;
        var worldPositionY = worldPosition.y;

        // get rectangle corners
        var topLeft = new Vector2(worldPositionX - Width * LevelGenerator.PIXEL_SIZE / 2, worldPositionY * LevelGenerator.PIXEL_SIZE + Height / 2);
        var bottomRight = new Vector2(worldPositionX + Width * LevelGenerator.PIXEL_SIZE / 2, worldPositionY - Height * LevelGenerator.PIXEL_SIZE / 2);

        return point.x >= topLeft.x && point.x <= bottomRight.x && point.y <= topLeft.y && point.y >= bottomRight.y;

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

        // If one rectangle is on left side of other
        if (topLeft.x > otherBottomRight.x || otherTopLeft.x > bottomRight.x)
        {
            return false;
        }
 
        // If one rectangle is above other
        if (bottomRight.y > otherTopLeft.y || otherBottomRight.y > topLeft.y)
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

    public bool IsAligned()
    {
        // Check if the cell is aligned to the grid
        var x = SimulationCell.transform.position.x;
        var y = SimulationCell.transform.position.y;

        var offsetX = Width * LevelGenerator.PIXEL_SIZE / 2;
        var offsetY = Height * LevelGenerator.PIXEL_SIZE / 2;

        // Get SimulationCell corners
        var topLeft = new Vector2(x - offsetX, y + offsetY);
        var bottomRight = new Vector2(x + offsetX, y - offsetY);

        // Check if the cell is aligned to the grid
        if (topLeft.x % LevelGenerator.cellSize != 0 || bottomRight.x % LevelGenerator.cellSize != 0 || topLeft.y % LevelGenerator.cellSize != 0 || bottomRight.y % LevelGenerator.cellSize != 0)
        {
            return false;
        }
        return true;
    }
}

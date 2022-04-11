using System;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject PhysicsCell;
    public GameObject SimulationCell;
    public GameObject DisplayCell;

    public Vector2 Position = new Vector2 (0, 0);
    public int Width;
    public int Height;

    public Cell(Vector2 position, int width, int height)
    {
        Width = width;
        Height = height;
        Position = position;
    }

    public void CreatePhysicsCellObject(int i, Sprite cellSprite)
    {
        PhysicsCell = new GameObject($"Simulation cell #{i}");
        PhysicsCell.transform.localPosition = new Vector2(Position.x * LevelGenerator.cellSize * LevelGenerator.PIXEL_SIZE, Position.y * LevelGenerator.cellSize * LevelGenerator.PIXEL_SIZE);
        PhysicsCell.transform.localScale = new Vector2(LevelGenerator.PIXEL_SIZE * Width, LevelGenerator.PIXEL_SIZE * Height);

        SpriteRenderer sprite = PhysicsCell.AddComponent<SpriteRenderer>();
        sprite.sprite = cellSprite;

        PhysicsCell.AddComponent<BoxCollider2D>();

        Rigidbody2D rigidbody = PhysicsCell.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0;
        rigidbody.freezeRotation = true;

    }

    public void CreateSimulationCellObject(int i, Sprite cellSprite)
    {
        SimulationCell = new GameObject($"Cell #{i}");
        SimulationCell.SetActive(false);
        SimulationCell.transform.localScale = new Vector2(LevelGenerator.PIXEL_SIZE * Width, LevelGenerator.PIXEL_SIZE * Height);
        SimulationCell.layer = LayerMask.NameToLayer("Cell");

        SpriteRenderer sprite = SimulationCell.AddComponent<SpriteRenderer>();
        sprite.sprite = cellSprite;
        sprite.color = new Color(0, 1, 1, 0.3f);
        sprite.sortingLayerName = "Render";

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

    public bool IsOverlapping(GameObject otherCell, float margin){

        var offsetX = Width * LevelGenerator.PIXEL_SIZE / 2 - margin;
        var offsetY = Height * LevelGenerator.PIXEL_SIZE / 2 - margin;

        // Get SimulationCell corners
        var topLeft = new Vector2(SimulationCell.transform.position.x - offsetX,
                                  SimulationCell.transform.position.y + offsetY);
        var bottomRight = new Vector2(SimulationCell.transform.position.x + offsetX,
                                      SimulationCell.transform.position.y - offsetY);

        // Do same for other cell
        var otherX = otherCell.transform.position.x;
        var otherY = otherCell.transform.position.y;

        var otherOffsetX = otherCell.transform.localScale.x / 2 - margin;
        var otherOffsetY = otherCell.transform.localScale.y / 2 - margin;

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

    public void CreateDisplayCellObject(Sprite cellSprite)
    {
        DisplayCell = new GameObject("Hallway cell");
        var offset = LevelGenerator.cellSize / 2;
        
        DisplayCell.transform.position = new Vector2(Position.x * LevelGenerator.cellSize * LevelGenerator.PIXEL_SIZE - offset,
                                                     Position.y * LevelGenerator.cellSize * LevelGenerator.PIXEL_SIZE - offset);
        DisplayCell.AddComponent<SpriteRenderer>().sprite = cellSprite;
        DisplayCell.GetComponent<SpriteRenderer>().color = Color.cyan;
        DisplayCell.transform.localScale = new Vector2(Width * LevelGenerator.cellSize, Height * LevelGenerator.cellSize);
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

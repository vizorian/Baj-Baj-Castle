using System;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject PhysicsCell;
    public GameObject SimulationCell;
    public GameObject DisplayCell;

    public Vector2 Position = new Vector2(0, 0);
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
        PhysicsCell.transform.localPosition = new Vector2(Position.x * LevelGenerator.CELL_SIZE, Position.y * LevelGenerator.CELL_SIZE);
        PhysicsCell.transform.localScale = new Vector2(LevelGenerator.CELL_SIZE * Width, LevelGenerator.CELL_SIZE * Height);

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
        SimulationCell.transform.localScale = new Vector2(LevelGenerator.CELL_SIZE * Width, LevelGenerator.CELL_SIZE * Height);
        SimulationCell.layer = LayerMask.NameToLayer("Cell");

        SpriteRenderer sprite = SimulationCell.AddComponent<SpriteRenderer>();
        sprite.sprite = cellSprite;
        sprite.color = new Color(0, 1, 1, 0.3f);
        sprite.sortingLayerName = "Render";

    }

    // TODO Fix this
    // HOW: probably resort to doing real position (world float) instead of cell position (int)
    // check if point is inside the cell's bounds
    public bool IsPointInside(Point point)
    {
        // calculate offsets
        var offsetX = Width / 2;
        var offsetY = Height / 2;

        // find cell bounds
        var minX = (int)Position.x - offsetX;
        var maxX = (int)Position.x + offsetX;
        var minY = (int)Position.y - offsetY;
        var maxY = (int)Position.y + offsetY;

        // check if point is inside the cell's bounds
        return (point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY);
    }

    public bool Overlaps(GameObject otherCell, float margin)
    {

        var offsetX = Width * LevelGenerator.CELL_SIZE / 2 - margin;
        var offsetY = Height * LevelGenerator.CELL_SIZE / 2 - margin;

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

    public bool Overlaps(Cell otherCell)
    {
        // get cell position from 
        var x = (int)(SimulationCell.transform.position.x / LevelGenerator.CELL_SIZE);
        var y = (int)(SimulationCell.transform.position.y / LevelGenerator.CELL_SIZE);

        // get cell size in world converted to int
        var width = Width / LevelGenerator.TILE_SIZE;
        var height = Height / LevelGenerator.TILE_SIZE;

        // get cell corner points
        var topLeft = new Point(x, y);
        var bottomRight = new Point(x + width, y + height);

        // get other cell size in world converted
        var otherTopLeft = new Point((int)(otherCell.Position.x), (int)(otherCell.Position.y));
        var otherBottomRight = new Point((int)(otherCell.Position.x + otherCell.Width), (int)(otherCell.Position.y + otherCell.Height));
        // print data

        // check if cells overlapp
        if (topLeft.X > otherBottomRight.X
            || otherTopLeft.X > bottomRight.X
            || bottomRight.Y > otherTopLeft.Y
            || otherBottomRight.Y > topLeft.Y)
        {
            return false;
        }
        return true;
    }

    public void CreateDisplayCellObject(Sprite cellSprite, Color color)
    {
        DisplayCell = new GameObject("Hallway cell");
        var offset = LevelGenerator.CELL_SIZE / 2;

        DisplayCell.transform.position = new Vector2(Position.x * LevelGenerator.CELL_SIZE - offset,
                                                     Position.y * LevelGenerator.CELL_SIZE - offset);
        DisplayCell.AddComponent<SpriteRenderer>().sprite = cellSprite;
        DisplayCell.GetComponent<SpriteRenderer>().color = color;
        DisplayCell.transform.localScale = new Vector2(Width * LevelGenerator.CELL_SIZE, Height * LevelGenerator.CELL_SIZE);
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

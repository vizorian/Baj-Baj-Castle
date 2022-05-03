using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Cell
{
    public GameObject DisplayCell;
    public int Height;
    public GameObject PhysicsCell;

    public Vector2 Position;
    public GameObject SimulationCell;
    public int Width;

    public Cell(Vector2 position, int width, int height)
    {
        Width = width;
        Height = height;
        Position = position;
    }

    public void CreatePhysicsCellObject(int i, Sprite cellSprite)
    {
        PhysicsCell = new GameObject($"Simulation cell #{i}")
        {
            transform =
            {
                localPosition = new Vector2(Position.x * LevelGenerator.CELL_SIZE, Position.y * LevelGenerator.CELL_SIZE),
                localScale = new Vector2(LevelGenerator.CELL_SIZE * Width, LevelGenerator.CELL_SIZE * Height)
            }
        };

        var sprite = PhysicsCell.AddComponent<SpriteRenderer>();
        sprite.sprite = cellSprite;

        PhysicsCell.AddComponent<BoxCollider2D>();

        var rigidbody = PhysicsCell.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0;
        rigidbody.freezeRotation = true;
    }

    public void CreateSimulationCellObject(int i, Sprite cellSprite)
    {
        SimulationCell = new GameObject($"Cell #{i}");
        SimulationCell.SetActive(false);
        SimulationCell.transform.localScale =
            new Vector2(LevelGenerator.CELL_SIZE * Width, LevelGenerator.CELL_SIZE * Height);
        SimulationCell.layer = LayerMask.NameToLayer("Cell");

        var sprite = SimulationCell.AddComponent<SpriteRenderer>();
        sprite.sprite = cellSprite;
        sprite.color = new Color(0, 1, 1, 0.3f);
        sprite.sortingLayerName = "Render";
    }

    // TODO Improve
    // HOW: probably resort to doing real position (world float) instead of cell position (int)
    // check if point is inside the cell's bounds
    public bool IsPointInside(Point point)
    {
        // calculate offsets
        var offsetX = Width / 2;
        var offsetY = Height / 2;

        // find cell bounds
        var minX = (int) Position.x - offsetX;
        var maxX = (int) Position.x + offsetX;
        var minY = (int) Position.y - offsetY;
        var maxY = (int) Position.y + offsetY;

        // check if point is inside the cell's bounds
        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
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
            return false;
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
        DisplayCell.transform.localScale =
            new Vector2(Width * LevelGenerator.CELL_SIZE, Height * LevelGenerator.CELL_SIZE);
    }

    public bool IsPartOf(HashSet<Triangle> triangles)
    {
        // Get all unique vertices from triangles
        var vertices = new HashSet<Point>();
        foreach (var triangle in triangles)
        {
            vertices.Add(triangle.Vertices[0]);
            vertices.Add(triangle.Vertices[1]);
            vertices.Add(triangle.Vertices[2]);
        }

        // Check if the cell is any of the vertices
        foreach (var vertex in vertices)
            if (vertex.X == SimulationCell.transform.position.x && vertex.Y == SimulationCell.transform.position.y)
                return true;

        return false;
    }
}
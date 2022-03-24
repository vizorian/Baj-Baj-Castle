using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject SimulationCell;
    public GameObject DisplayCell;
    public Collider2D DisplayCollider;

    public Vector2 Position = new Vector2 (0, 0);
    public float Width;
    public float Height;

    public Cell(Vector2 position, float width, float height)
    {
        Width = width;
        Height = height;
        Position = position;
    }
}

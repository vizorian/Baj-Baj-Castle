using System;
using UnityEngine;

public class Cell
{
    public Vector2 Position = new Vector2 (0, 0);
    public float Width;
    public float Height;

    //public Vector2 TL;
    //public Vector2 BR;

    public Cell(Vector2 position, float width, float height)
    {
        Width = width;
        Height = height;
        Position = position;
        //UpdateCellCorners();
    }

    //private void UpdateCellCorners()
    //{
    //    TL = new Vector2(Position.x - Width / 2, Position.y + Height / 2);
    //    BR = new Vector2(Position.x + Width / 2, Position.y - Height / 2);
    //}

    //public bool IsOverlapping(Cell cell)
    //{
    //    // If A is left of B
    //    if (TL.x >= cell.BR.x || cell.TL.x >= BR.x)
    //    {
    //        return false;
    //    }

    //    // If A is above B
    //    if (BR.y >= cell.TL.y || cell.BR.y >= TL.y)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //public void Move(Vector2 move, int tileSize)
    //{
    //    Position.x = LevelGenerator.RoundNumber(Position.x + Mathf.Round(move.x) * tileSize, tileSize);
    //    Position.y = LevelGenerator.RoundNumber(Position.y + Mathf.Round(move.y) * tileSize, tileSize);

    //    //Position.x = Position.x + Mathf.Round(move.x);
    //    //Position.y = Position.y + Mathf.Round(move.y);
    //    UpdateCellCorners();
    //}

    public override string ToString()
    {
        //return $"Cell {Position} {TL} {BR}";
        return $"Cell {Position}";
    }
}

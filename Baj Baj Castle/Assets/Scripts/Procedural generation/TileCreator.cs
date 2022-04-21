using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCreator
{
    private Dictionary<string, Tile> tiles;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;
    private Tilemap collisionTilemap;

    public TileCreator(Tilemap floorTilemap, Tilemap decorationTilemap, Tilemap collisionTilemap)
    {
        tiles = GameAssets.instance.tiles;
        this.floorTilemap = floorTilemap;
        this.decorationTilemap = decorationTilemap;
        this.collisionTilemap = collisionTilemap;
    }

    public void CreateTiles(List<Cell> cells)
    {
        foreach (var cell in cells)
        {
            // Create floor tile
            var floorTile = tiles["Floor"];
            var wallTile = tiles["Wall"];

            if (cell.Width == 1 && cell.Height == 1) // Tile is hallway
            {
                // if tile is not empty
                if (floorTilemap.GetTile(new Vector3Int((int)cell.Position.x - 1, (int)cell.Position.y - 1, 0)) == null)
                {
                    floorTilemap.SetTile(new Vector3Int((int)cell.Position.x - 1, (int)cell.Position.y - 1, 0), floorTile);
                }
            }
            else // Tile is room
            {
                var halfWidth = cell.Width / 2;
                var halfHeight = cell.Height / 2;

                int startingX = Mathf.RoundToInt(cell.Position.x) - halfWidth;
                int startingY = Mathf.RoundToInt(cell.Position.y) - halfHeight;
                int endingX = startingX + cell.Width;
                int endingY = startingY + cell.Height;

                for (var x = startingX; x < endingX; x++)
                {
                    for (var y = startingY; y < endingY; y++)
                    {
                        floorTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                }
            }
        }
    }
}

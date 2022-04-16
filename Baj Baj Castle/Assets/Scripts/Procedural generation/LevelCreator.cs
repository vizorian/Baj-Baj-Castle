using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCreator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateTiles(List<Cell> cells, Tilemap floorTilemap, Tilemap collisionTilemap)
    {
        foreach (var cell in cells)
        {
            var x = cell.Position.x;
            var y = cell.Position.y;
            var width = cell.Width;
            var height = cell.Height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var point = new Point(x + i, y + j);
                    var tile = floorTilemap.GetTile(new Vector3Int((int)point.X, (int)point.Y, 0));
                    if (tile == null)
                    {
                        floorTilemap.SetTile(new Vector3Int((int)point.X, (int)point.Y, 0), floorTilemap.GetTile(new Vector3Int(0, 0, 0)));
                    }
                    tile = collisionTilemap.GetTile(new Vector3Int((int)point.X, (int)point.Y, 0));
                    if (tile == null)
                    {
                        collisionTilemap.SetTile(new Vector3Int((int)point.X, (int)point.Y, 0), collisionTilemap.GetTile(new Vector3Int(0, 0, 0)));
                    }
                }
            }
        }
    }
}

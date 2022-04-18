using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public GameObject GridObject;
    public Sprite CellSprite;
    public bool IsDebug;

    public Dictionary<string, Tile> TileDictionary;
    private LevelGenerator levelGenerator;
    private TileCreator tileCreator;
    public int Level = 1;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;
    private Tilemap collisionTilemap;

    public void GenerateLevel(int i)
    {
        InstantiateLevelGenerator();
        levelGenerator.GenerateLevel(i, IsDebug, CellSprite);

        var rooms = levelGenerator.Rooms;
        var hallways = levelGenerator.Hallways;

        if (tileCreator == null)
        {
            InstantiateTileCreator();
        }
        tileCreator.CreateTiles(rooms, floorTilemap, collisionTilemap);

        Level++;
    }

    private void InstantiateLevelGenerator()
    {
        levelGenerator = gameObject.AddComponent<LevelGenerator>();
        levelGenerator.CellSprite = CellSprite;
        levelGenerator.IsDebug = IsDebug;
    }

    private void InstantiateTileCreator()
    {
        var floorTileObject = GridObject.transform.Find("Floor").gameObject;
        var decorationTileObject = GridObject.transform.Find("Decoration").gameObject;
        var collisionTileObject = GridObject.transform.Find("Collision").gameObject;

        floorTilemap = floorTileObject.GetComponent<Tilemap>();
        decorationTilemap = decorationTileObject.GetComponent<Tilemap>();
        collisionTilemap = collisionTileObject.GetComponent<Tilemap>();

        tileCreator = gameObject.AddComponent<TileCreator>();
    }
}

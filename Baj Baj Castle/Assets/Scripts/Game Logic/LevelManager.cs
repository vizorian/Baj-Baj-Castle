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
    public List<Room> Rooms;
    public List<TileData> Hallways;

    public bool IsGeneratingLevel = false;
    public bool IsGenerated = false;

    private void Update()
    {
        if (!IsGenerated && IsGeneratingLevel)
        {
            if (levelGenerator.IsCompleted)
            {
                IsGenerated = true;
                IsGeneratingLevel = false;
                GenerateLevelTiles();
            }
        }
    }

    public void GenerateLevel(int i)
    {
        IsGeneratingLevel = true;
        InstantiateLevelGenerator();
        levelGenerator.GenerateLevel(i, IsDebug, CellSprite);
    }

    public void GenerateLevelTiles()
    {
        var roomCells = levelGenerator.Rooms;
        var hallwayCells = levelGenerator.Hallways;

        levelGenerator.Clear();
        if (tileCreator == null)
        {
            InstantiateTileCreator();
        }
        Rooms = tileCreator.CreateRooms(roomCells);
        Hallways = tileCreator.CreateHallways(hallwayCells);

        levelGenerator.Clear();

        tileCreator.UpdateTiles(Rooms);

        levelGenerator.Reset();
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

        tileCreator = new TileCreator(floorTilemap, decorationTilemap, collisionTilemap);
    }
}

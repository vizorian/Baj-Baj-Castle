using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    private GameObject gridObject;
    private Sprite cellSprite;
    private bool isDebug;

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

    public void InstantiateComponent(Sprite cellSprite, GameObject gridObject, bool isDebug)
    {
        this.isDebug = isDebug;
        this.gridObject = gridObject;
        this.cellSprite = cellSprite;

        levelGenerator = gameObject.AddComponent<LevelGenerator>();

        var floorTilemap = this.gridObject.transform.Find("Floor").GetComponent<Tilemap>();
        var decorationTilemap = this.gridObject.transform.Find("Decoration").GetComponent<Tilemap>();
        var collisionTilemap = this.gridObject.transform.Find("Collision").GetComponent<Tilemap>();
        tileCreator = new TileCreator(floorTilemap, decorationTilemap, collisionTilemap);
    }

    public void GenerateLevel(int level)
    {
        IsGenerated = false;
        IsGeneratingLevel = true;

        levelGenerator.GenerateLevel(level, isDebug, cellSprite);
    }

    public void Clear()
    {
        if (floorTilemap != null)
        {
            floorTilemap.ClearAllTiles();
        }

        if (decorationTilemap != null)
        {
            decorationTilemap.ClearAllTiles();
        }

        if (collisionTilemap != null)
        {
            collisionTilemap.ClearAllTiles();
        }

        tileCreator.Clear();
    }

    public void GenerateLevelTiles()
    {
        var roomCells = levelGenerator.Rooms;
        var hallwayCells = levelGenerator.Hallways;
        levelGenerator.Clear();

        Rooms = tileCreator.CreateRooms(roomCells);
        // tileCreator.CreateRoomTiles(Rooms);
        Hallways = tileCreator.CreateHallways(hallwayCells, Rooms);

        // tileCreator.CreateHallwayTiles(Hallways);

        // TODO task might be useless?
        tileCreator.FindNeighbours(Rooms);
        tileCreator.CreateTiles(Rooms, Hallways);

        levelGenerator.Reset();
        Level++;
    }
}

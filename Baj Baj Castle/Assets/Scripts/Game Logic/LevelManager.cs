using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Room StartRoom;
    public Room ExitRoom;
    public List<GameObject> Items = new List<GameObject>();
    public List<GameObject> Actors = new List<GameObject>();
    public List<GameObject> Triggers = new List<GameObject>();
    public List<GameObject> Objects = new List<GameObject>();
    public GameObject Player;

    public bool IsGeneratingLevel = false;
    public bool IsGenerated = false;
    public bool IsLoaded = false;
    public bool StartingLevelPopulation = false;
    public bool IsPopulated = false;

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
        tileCreator = new TileCreator(floorTilemap, decorationTilemap, collisionTilemap, isDebug);
    }

    public void GenerateLevel(int level)
    {
        IsGenerated = false;
        IsGeneratingLevel = true;

        levelGenerator.GenerateLevel(level, isDebug, cellSprite);
    }

    public void Cleanup()
    {
        tileCreator.Cleanup();
        levelGenerator.Cleanup();

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

        if (Rooms != null)
        {
            Rooms.Clear();
        }

        if (Hallways != null)
        {
            Hallways.Clear();
        }

        foreach (var item in Items)
        {
            Destroy(item);
        }
        Items.Clear();

        foreach (var actor in Actors)
        {
            Destroy(actor);
        }
        Actors.Clear();

        foreach (var trigger in Triggers)
        {
            Destroy(trigger);
        }
        Triggers.Clear();

        foreach (var object_ in Objects)
        {
            Destroy(object_);
        }
        Objects.Clear();

        if (Player != null)
        {
            Player.SetActive(false);
        }

        IsPopulated = false;
        IsGenerated = false;
        IsGeneratingLevel = false;
        IsLoaded = false;
        StartingLevelPopulation = false;
    }

    // TODO Fill level with content
    public void PopulateLevel(int level)
    {
        StartingLevelPopulation = true;

        CreateEndpoints();

        IsPopulated = true;
    }

    private void CreateEndpoints()
    {
        // set starting room as random room
        StartRoom = Rooms[UnityEngine.Random.Range(0, Rooms.Count)];

        // set exit room as random room that is not the starting room and is not a neighbor of the starting room and is furthest away

        do
        {
            ExitRoom = Rooms[UnityEngine.Random.Range(0, Rooms.Count)];
        }
        while (ExitRoom == StartRoom || ExitRoom.Neighbours.Contains(StartRoom));

        // spawn player at center of starting room
        if (Player != null)
        {
            Player.transform.position = new Vector3(StartRoom.Center.X * LevelGenerator.CELL_SIZE, StartRoom.Center.Y * LevelGenerator.CELL_SIZE, 0);
            Player.SetActive(true);
        }
        else
        {
            Player = Instantiate(GameAssets.Instance.playerPrefab, new Vector2(StartRoom.Center.X * LevelGenerator.CELL_SIZE, StartRoom.Center.Y * LevelGenerator.CELL_SIZE), Quaternion.identity);
            Camera.main.GetComponent<CameraMovement>().target = Player.transform;
            Instantiate(GameAssets.Instance.itemPrefabs.First(i => i.name.Contains("Knife")), Player.transform.position, Quaternion.identity);
            GameManager.Instance.Canvas.transform.Find("InventoryBar").gameObject.SetActive(true);
            var healthBar = GameManager.Instance.Canvas.transform.Find("HealthBar").gameObject;
            // healthBar.GetComponent<HealthBar>().Player = Player.GetComponent<Player>();
            healthBar.SetActive(true);
        }

        // create exit from level at center of exit room
        Triggers.Add(Instantiate(GameAssets.Instance.triggers[0], new Vector2(ExitRoom.Center.X * LevelGenerator.CELL_SIZE, ExitRoom.Center.Y * LevelGenerator.CELL_SIZE), Quaternion.identity));
        tileCreator.OverrideTile(ExitRoom.Center, "Exit");
    }

    public void GenerateLevelTiles()
    {
        var roomCells = levelGenerator.Rooms;
        var hallwayCells = levelGenerator.Hallways;

        Rooms = tileCreator.CreateRooms(roomCells);
        Hallways = tileCreator.CreateHallways(hallwayCells, Rooms);
        levelGenerator.Cleanup();

        // tileCreator.CreateHallwayTiles(Hallways);

        tileCreator.FindNeighbouringRooms(Rooms);
        tileCreator.CreateTiles(Rooms, Hallways);

        Level++;
        IsLoaded = true;
    }
}

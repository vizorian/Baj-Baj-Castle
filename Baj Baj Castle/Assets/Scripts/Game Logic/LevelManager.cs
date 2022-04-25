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
        Debug.Log("Starting population");
        StartingLevelPopulation = true;

        Debug.Log("Creating entrance and exit");
        CreateEndpoints();

        Debug.Log("Populating rooms");
        foreach (var room in Rooms)
        {
            if (room != StartRoom)
            {
                PopulateRoom(room);
            }
        }

        IsPopulated = true;
    }

    private void PopulateRoom(Room room)
    {
        var limit = room.Area - 9;
        Debug.Log("Populating room: " + room.Id);
        var level = GameManager.Instance.Level;

        // Enemy spawning calculations
        var enemySpawnChance = 0.5f + (level * 0.1f);
        int enemySpawnCount = 0;
        while (UnityEngine.Random.Range(0f, 1f) < enemySpawnChance)
        {
            if (enemySpawnCount > limit)
            {
                break;
            }
            enemySpawnCount++;
            enemySpawnChance -= 0.1f;
        }
        limit -= enemySpawnCount;
        Debug.Log("Enemy spawn count: " + enemySpawnCount);

        // Object spawning calculations
        var objectSpawnChance = 0.5f + (level * 0.1f);
        int objectSpawnCount = 0;
        while (UnityEngine.Random.Range(0f, 1f) < objectSpawnChance)
        {
            if (objectSpawnCount > limit)
            {
                break;
            }
            objectSpawnCount++;
            objectSpawnChance -= 0.05f;
        }
        if (objectSpawnCount == 0)
        {
            objectSpawnCount = 1;
        }
        Debug.Log("Object spawn count: " + objectSpawnCount);

        GameObject prefab;

        // Enemy spawning
        for (int i = 0; i < enemySpawnCount; i++)
        {
            prefab = GameAssets.Instance.enemyPrefabs[UnityEngine.Random.Range(0, GameAssets.Instance.enemyPrefabs.Count)];
            // TODO spawn enemy with pricing
            var enemy = Instantiate(prefab, room.GetRandomTile(), Quaternion.identity);
            enemy.GetComponent<Actor>().room = room;
            Actors.Add(enemy);
        }

        // Object spawning
        var chestless = GameAssets.Instance.objectPrefabs.Where(x => x.name != "Chest_I").ToList();
        for (int i = 0; i < objectSpawnCount; i++)
        {
            prefab = GameAssets.Instance.objectPrefabs[UnityEngine.Random.Range(0, GameAssets.Instance.objectPrefabs.Count)];
            if (prefab.name == "Chest_I")
            {
                // chance for chest
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f + level * 0.1f)
                {
                    prefab = chestless[UnityEngine.Random.Range(0, chestless.Count)];
                }
            }

            var object_ = Instantiate(prefab, room.GetRandomTile(), Quaternion.identity);
            Objects.Add(object_);
        }

        // Trigger spawning
        // create a room sized trigger at the center of the room
        var trigger = Instantiate(GameAssets.Instance.triggers[1], room.CenterPosition, Quaternion.identity);
        trigger.transform.localScale = new Vector2(room.WidthToWorld, room.HeightToWorld);

        // re-center the trigger to the center of the room
        var triggerCorner = new Vector2(trigger.transform.position.x - trigger.transform.lossyScale.x / 2,
                                        trigger.transform.position.y + trigger.transform.lossyScale.y / 2);

        if (triggerCorner.x < room.TopLeftCorner.x) // if trigger is too much left
        {
            trigger.transform.position += new Vector3(LevelGenerator.CELL_SIZE / 2, 0);
        }
        else if (triggerCorner.x > room.TopLeftCorner.x) // if trigger is too much right
        {
            trigger.transform.position += new Vector3(-LevelGenerator.CELL_SIZE / 2, 0);
        }

        if (triggerCorner.y < room.TopLeftCorner.y) // if trigger is too much down
        {
            trigger.transform.position += new Vector3(0, LevelGenerator.CELL_SIZE / 2);
        }
        else if (triggerCorner.y > room.TopLeftCorner.y) // if trigger is too much up
        {
            trigger.transform.position += new Vector3(0, -LevelGenerator.CELL_SIZE / 2);
        }
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
            Player = Instantiate(GameAssets.Instance.playerPrefab,
                                 new Vector2(StartRoom.Center.X * LevelGenerator.CELL_SIZE, StartRoom.Center.Y * LevelGenerator.CELL_SIZE),
                                 Quaternion.identity);
            Camera.main.GetComponent<CameraMovement>().target = Player.transform;

            // starter weapon
            Instantiate(GameAssets.Instance.itemPrefabs.First(i => i.name.Contains("Knife_Wooden")),
                        new Vector3(Player.transform.position.x + LevelGenerator.CELL_SIZE, Player.transform.position.y),
                        Quaternion.identity);

            var gameCanvas = GameObject.Find("GameCanvas");
            gameCanvas.transform.Find("InventoryBar").gameObject.SetActive(true);
            var healthBar = gameCanvas.transform.Find("HealthBar").gameObject;
            healthBar.SetActive(true);
        }

        StartRoom.IsActive = false;
        StartRoom.IsCleared = true;

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

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
    private GameObject exitTriggerObject;
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

    public static LevelManager Instance { get; private set; }

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

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

    public void AddItem(GameObject item)
    {
        LevelManager.Instance.Items.Add(item);
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

    public void PopulateLevel(int level)
    {
        StartingLevelPopulation = true;

        // Create entrance, exit and player
        CreateEndpoints();

        // Populate all rooms, except starting room
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
        var level = GameManager.Instance.Level;

        // Enemy spawning calculations
        var enemySpawnChance = 0.5f + (level * 0.1f);
        int enemySpawnCount = 0;
        while (UnityEngine.Random.Range(0f, 1f) < enemySpawnChance)
        {
            if (enemySpawnCount >= limit)
            {
                break;
            }
            enemySpawnCount++;
            enemySpawnChance -= 0.1f;
        }
        if (enemySpawnCount == 0)
        {
            enemySpawnCount = 1 + UnityEngine.Random.Range(0, 2);
        }
        limit -= enemySpawnCount;

        // Object spawning calculations
        var objectSpawnChance = 0.7f + (level * 0.05f);
        int objectSpawnCount = level + 1;
        while (UnityEngine.Random.Range(0f, 1f) < objectSpawnChance)
        {
            if (objectSpawnCount >= limit)
            {
                break;
            }
            objectSpawnCount++;
        }

        GameObject prefab;

        // Enemy spawning
        for (int i = 0; i < enemySpawnCount; i++)
        {
            var easyEnemies = GameAssets.Instance.enemyPrefabs.Where(e => e.name == "Gobbo" || e.name == "Goblin").ToList();
            prefab = GameAssets.Instance.enemyPrefabs[UnityEngine.Random.Range(0, GameAssets.Instance.enemyPrefabs.Count)];
            if (!easyEnemies.Any(e => e.name == prefab.name))
            {
                if (UnityEngine.Random.Range(0f, 1f) < 9f - level * 0.1f)
                {
                    prefab = easyEnemies[UnityEngine.Random.Range(0, easyEnemies.Count)];
                }
            }
            var enemy = Instantiate(prefab, room.GetRandomTile(2), Quaternion.identity);
            Actors.Add(enemy);
            room.Actors.Add(enemy.GetComponent<Actor>());
        }

        // Object spawning
        var crates = GameAssets.Instance.objectPrefabs.Where(o => o.name.Contains("Crate")).ToList();
        var chestless = GameAssets.Instance.objectPrefabs.Where(o => o.name != "Chest_I").ToList();
        for (int i = 0; i < objectSpawnCount; i++)
        {
            prefab = GameAssets.Instance.objectPrefabs[UnityEngine.Random.Range(0, GameAssets.Instance.objectPrefabs.Count)];
            if (prefab.name == "Chest_I")
            {
                // chance for chest
                if (UnityEngine.Random.Range(0f, 1f) < 0.4f + level * 0.1f)
                {
                    prefab = chestless[UnityEngine.Random.Range(0, chestless.Count)];
                }
            }
            else if (prefab.name.Contains("Table"))
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.75f)
                {
                    prefab = crates[UnityEngine.Random.Range(0, crates.Count)];
                }
            }

            var object_ = Instantiate(prefab, room.GetRandomTile(2), Quaternion.identity);
            Objects.Add(object_);
        }

        // Trigger spawning
        if (enemySpawnCount > 0)
        {
            // create a room sized trigger at the center of the room
            var triggerObject = Instantiate(GameAssets.Instance.triggers[1], room.CenterPosition, Quaternion.identity);
            Triggers.Add(triggerObject);
            var trigger = triggerObject.GetComponent<RoomTrigger>();
            if (room == ExitRoom)
            {
                trigger.Exit = exitTriggerObject.GetComponent<ExitTrigger>();
            }
            room.RoomTrigger = trigger;
            trigger.ParentRoom = room;

            triggerObject.transform.localScale = new Vector2(room.WidthToWorld, room.HeightToWorld);

            // re-center the trigger to the center of the room via top left corner tile
            var moveX = new Vector3(LevelGenerator.CELL_SIZE / 2, 0);
            while ((triggerObject.transform.position.x - triggerObject.transform.lossyScale.x / 2) > room.TopLeftCorner.x + LevelGenerator.CELL_SIZE / 4)
            {
                triggerObject.transform.position -= moveX;
            }

            while ((triggerObject.transform.position.x - triggerObject.transform.lossyScale.x / 2) < room.TopLeftCorner.x - LevelGenerator.CELL_SIZE / 4)
            {
                triggerObject.transform.position += moveX;
            }

            var moveY = new Vector3(0, LevelGenerator.CELL_SIZE / 2);
            while ((triggerObject.transform.position.y + triggerObject.transform.lossyScale.y / 2) > room.TopLeftCorner.y + LevelGenerator.CELL_SIZE / 4)
            {
                triggerObject.transform.position -= moveY;
            }

            while ((triggerObject.transform.position.y + triggerObject.transform.lossyScale.y / 2) < room.TopLeftCorner.y - LevelGenerator.CELL_SIZE / 4)
            {
                triggerObject.transform.position += moveY;
            }
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
        exitTriggerObject = Instantiate(GameAssets.Instance.triggers[0],
                                 new Vector2(ExitRoom.Center.X * LevelGenerator.CELL_SIZE, ExitRoom.Center.Y * LevelGenerator.CELL_SIZE),
                                 Quaternion.identity);
        Triggers.Add(exitTriggerObject);
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

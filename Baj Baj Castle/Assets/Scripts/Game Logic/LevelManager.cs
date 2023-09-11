using System.Collections.Generic;
using System.Linq;
using Actor_Behaviour;
using Game_Logic.Tiles;
using Game_Logic.Triggers;
using JetBrains.Annotations;
using Procedural_generation;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileData = Game_Logic.Tiles.TileData;

namespace Game_Logic;

public class LevelManager : MonoBehaviour
{
    public int Level = 1;
    public List<TileData> Hallways;
    public List<Room> Rooms;
    public Room StartRoom;
    public Room ExitRoom;
    public List<GameObject> Actors = new List<GameObject>();
    public GameObject Player;
    public List<GameObject> Items = new List<GameObject>();
    public List<GameObject> Objects = new List<GameObject>();
    public List<GameObject> Triggers = new List<GameObject>();

    private LevelGenerator levelGenerator;
    private TileCreator tileCreator;
    private Sprite cellSprite;
    private Tilemap collisionTilemap;
    private Tilemap decorationTilemap;
    private GameObject exitTriggerObject;
    private Tilemap floorTilemap;
    private GameObject gridObject;

    private bool isDebug;
    public bool IsGenerated;
    public bool IsGeneratingLevel;
    public bool IsLoaded;
    public bool IsPopulated;
    public bool StartingLevelPopulation;

    public static LevelManager Instance { get; private set; }

    [UsedImplicitly]
    private void Update()
    {
        if (!IsGenerated && IsGeneratingLevel)
            if (levelGenerator.IsCompleted)
            {
                IsGenerated = true;
                IsGeneratingLevel = false;
                GenerateLevelTiles();
            }
    }

    public void InstantiateComponent(Sprite newCellSprite, GameObject newGridObject, bool newIsDebug)
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        isDebug = newIsDebug;
        gridObject = newGridObject;
        cellSprite = newCellSprite;

        levelGenerator = gameObject.AddComponent<LevelGenerator>();

        floorTilemap = this.gridObject.transform.Find("Floor").GetComponent<Tilemap>();
        decorationTilemap = this.gridObject.transform.Find("Decoration").GetComponent<Tilemap>();
        collisionTilemap = this.gridObject.transform.Find("Collision").GetComponent<Tilemap>();
        tileCreator = new TileCreator(floorTilemap, decorationTilemap, collisionTilemap, newIsDebug);
    }

    // Generate level
    public void GenerateLevel(int level)
    {
        IsGenerated = false;
        IsGeneratingLevel = true;

        levelGenerator.GenerateLevel(level, isDebug, cellSprite);
    }

    // Add item to item list
    public void AddItem(GameObject item)
    {
        Instance.Items.Add(item);
    }

    // Cleanup process
    public void Cleanup()
    {
        tileCreator.Cleanup();
        levelGenerator.Cleanup();

        if (floorTilemap != null) floorTilemap.ClearAllTiles();

        if (decorationTilemap != null) decorationTilemap.ClearAllTiles();

        if (collisionTilemap != null) collisionTilemap.ClearAllTiles();

        if (Rooms != null) Rooms.Clear();

        if (Hallways != null) Hallways.Clear();

        foreach (var item in Items) Destroy(item);
        Items.Clear();

        foreach (var actor in Actors) Destroy(actor);
        Actors.Clear();

        foreach (var trigger in Triggers) Destroy(trigger);
        Triggers.Clear();

        foreach (var @object in Objects) Destroy(@object);
        Objects.Clear();

        // Disable player
        if (Player != null) Player.SetActive(false);

        IsPopulated = false;
        IsGenerated = false;
        IsGeneratingLevel = false;
        IsLoaded = false;
        StartingLevelPopulation = false;
    }

    // Populate level
    public void PopulateLevel(int level)
    {
        StartingLevelPopulation = true;

        // Create endpoints
        CreateEndpoints();

        // Populate all rooms, except starting room
        foreach (var room in Rooms)
            if (!Equals(room, StartRoom))
                PopulateRoom(room);

        IsPopulated = true;
    }

    // Populate room
    private void PopulateRoom(Room room)
    {
        var limit = room.Area - 9;
        var level = GameManager.Instance.Level;

        // Enemy spawning calculations
        var enemySpawnChance = 0.5f + level * 0.1f;
        var enemySpawnCount = 0;
        while (Random.Range(0f, 1f) < enemySpawnChance)
        {
            if (enemySpawnCount >= limit) break;
            enemySpawnCount++;
            enemySpawnChance -= 0.1f;
        }

        if (enemySpawnCount == 0) enemySpawnCount = 1 + Random.Range(0, 2);
        limit -= enemySpawnCount;

        // Object spawning calculations
        var objectSpawnChance = 0.7f + level * 0.05f;
        var objectSpawnCount = level + 1;
        while (Random.Range(0f, 1f) < objectSpawnChance)
        {
            if (objectSpawnCount >= limit) break;
            objectSpawnCount++;
        }

        GameObject prefab;

        // Enemy spawning
        for (var i = 0; i < enemySpawnCount; i++)
        {
            var easyEnemies = GameAssets.Instance.EnemyPrefabs.Where(e => e.name == "Gobbo" || e.name == "Goblin")
                .ToList();
            prefab = GameAssets.Instance.EnemyPrefabs[Random.Range(0, GameAssets.Instance.EnemyPrefabs.Count)];
            if (easyEnemies.All(e => e.name != prefab.name))
                if (Random.Range(0f, 1f) < 1f - level * 0.1f)
                    prefab = easyEnemies[Random.Range(0, easyEnemies.Count)];
            var enemy = Instantiate(prefab, room.GetRandomTile(2), Quaternion.identity);
            Actors.Add(enemy);
            room.Actors.Add(enemy.GetComponent<Actor>());
        }

        // Object spawning
        var crates = GameAssets.Instance.ObjectPrefabs.Where(o => o.name.Contains("Crate")).ToList();
        var chestless = GameAssets.Instance.ObjectPrefabs.Where(o => o.name != "Chest_I").ToList();
        for (var i = 0; i < objectSpawnCount; i++)
        {
            prefab = GameAssets.Instance.ObjectPrefabs[Random.Range(0, GameAssets.Instance.ObjectPrefabs.Count)];
            if (prefab.name == "Chest_I")
            {
                // chance for chest
                if (Random.Range(0f, 1f) < 0.4f + level * 0.1f) prefab = chestless[Random.Range(0, chestless.Count)];
            }
            else if (prefab.name.Contains("Table"))
            {
                if (Random.Range(0f, 1f) < 0.75f) prefab = crates[Random.Range(0, crates.Count)];
            }

            var @object = Instantiate(prefab, room.GetRandomTile(2), Quaternion.identity);
            Objects.Add(@object);
        }

        // Trigger spawning
        // create a room sized trigger at the center of the room
        var triggerObject = Instantiate(GameAssets.Instance.Triggers[1], room.CenterPosition, Quaternion.identity);
        Triggers.Add(triggerObject);
        var trigger = triggerObject.GetComponent<RoomTrigger>();
        if (Equals(room, ExitRoom)) trigger.Exit = exitTriggerObject.GetComponent<ExitTrigger>();
        room.RoomTrigger = trigger;
        trigger.ParentRoom = room;

        triggerObject.transform.localScale = new Vector2(room.WidthToWorld, room.HeightToWorld);

        // re-center the trigger to the center of the room via top left corner tile
        var moveX = new Vector3(LevelGenerator.CELL_SIZE / 2, 0);
        while (triggerObject.transform.position.x - triggerObject.transform.lossyScale.x / 2 >
               room.TopLeftCorner.x + LevelGenerator.CELL_SIZE / 4) triggerObject.transform.position -= moveX;

        while (triggerObject.transform.position.x - triggerObject.transform.lossyScale.x / 2 <
               room.TopLeftCorner.x - LevelGenerator.CELL_SIZE / 4) triggerObject.transform.position += moveX;

        var moveY = new Vector3(0, LevelGenerator.CELL_SIZE / 2);
        while (triggerObject.transform.position.y + triggerObject.transform.lossyScale.y / 2 >
               room.TopLeftCorner.y + LevelGenerator.CELL_SIZE / 4) triggerObject.transform.position -= moveY;

        while (triggerObject.transform.position.y + triggerObject.transform.lossyScale.y / 2 <
               room.TopLeftCorner.y - LevelGenerator.CELL_SIZE / 4) triggerObject.transform.position += moveY;
    }

    // Create level endpoints
    private void CreateEndpoints()
    {
        // Set starting room as random room
        StartRoom = Rooms[Random.Range(0, Rooms.Count)];

        // Set exit room as random room that is not the starting room, is not a neigbour and is furthest away
        do
        {
            ExitRoom = Rooms[Random.Range(0, Rooms.Count)];
        } while (Equals(ExitRoom, StartRoom) || ExitRoom.Neighbours.Contains(StartRoom));

        // Move player to center of starting room
        if (Player != null)
        {
            Player.transform.position = new Vector3(StartRoom.Center.X * LevelGenerator.CELL_SIZE,
                StartRoom.Center.Y * LevelGenerator.CELL_SIZE, 0);
            // Activate player
            Player.SetActive(true);
        }
        else
        {
            // Create player
            Player = Instantiate(GameAssets.Instance.PlayerPrefab,
                new Vector2(StartRoom.Center.X * LevelGenerator.CELL_SIZE,
                    StartRoom.Center.Y * LevelGenerator.CELL_SIZE),
                Quaternion.identity);
            Camera.main.GetComponent<CameraMovement>().Target = Player.transform;

            // Create starter weapon
            Instantiate(GameAssets.Instance.ItemPrefabs.First(i => i.name.Contains("Knife_Wooden")),
                new Vector3(Player.transform.position.x + LevelGenerator.CELL_SIZE, Player.transform.position.y),
                Quaternion.identity);

            // Create display elements
            var gameCanvas = GameObject.Find("GameCanvas");
            gameCanvas.transform.Find("InventoryBar").gameObject.SetActive(true);
            var healthBar = gameCanvas.transform.Find("HealthBar").gameObject;
            healthBar.SetActive(true);
        }

        StartRoom.IsActive = false;
        StartRoom.IsCleared = true;

        // Create exit from level at center of exit room
        exitTriggerObject = Instantiate(GameAssets.Instance.Triggers[0],
            new Vector2(ExitRoom.Center.X * LevelGenerator.CELL_SIZE, ExitRoom.Center.Y * LevelGenerator.CELL_SIZE),
            Quaternion.identity);
        Triggers.Add(exitTriggerObject);
        tileCreator.OverrideTile(ExitRoom.Center, "Exit");
    }

    // Generate level tiles
    public void GenerateLevelTiles()
    {
        var roomCells = levelGenerator.Rooms;
        var hallwayCells = levelGenerator.Hallways;

        // Create rooms from room cells
        Rooms = tileCreator.CreateRooms(roomCells);
        // Create hallways from hallway cells
        Hallways = tileCreator.CreateHallways(hallwayCells, Rooms);
        // Cleanup level generator
        levelGenerator.Cleanup();

        // Find neighbours for rooms
        TileCreator.FindNeighbouringRooms(Rooms);

        // Create tiles
        tileCreator.CreateTiles(Rooms, Hallways);

        Level++;
        IsLoaded = true;
    }
}
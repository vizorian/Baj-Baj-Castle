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
    private bool isGenerated = false;
    private int level = 1;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;
    private Tilemap collisionTilemap;

    // Start is called before the first frame update
    void Start()
    {
        levelGenerator = gameObject.AddComponent<LevelGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        // if (!isGenerated)
        // {
        //     isGenerated = true;
        //     GenerateLevel(1);
        // }
    }

    public void GenerateLevel(int i)
    {
        levelGenerator.GenerateLevel(i, IsDebug, CellSprite);

        var rooms = levelGenerator.Rooms;
        var hallways = levelGenerator.Hallways;

        if (tileCreator == null)
        {
            InstantiateTileCreator();
        }
        tileCreator.CreateTiles(rooms, floorTilemap, collisionTilemap);

        level++;
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

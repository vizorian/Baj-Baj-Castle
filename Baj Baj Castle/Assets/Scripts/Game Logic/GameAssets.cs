using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;
    public static GameAssets Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<GameAssets>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    public void Awake()
    {
        var tileArray = Resources.LoadAll<Tile>("Art/Levels/Tiles/");
        tiles = new Dictionary<string, Tile>();
        foreach (var tile in tileArray)
        {
            tiles[tile.name] = tile;
        }
    }

    public GameObject floatingTextObject;
    public GameObject tooltipObject;
    public Dictionary<string, Tile> tiles;
}


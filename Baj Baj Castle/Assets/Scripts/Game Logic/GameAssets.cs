using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _instance;
    public static GameAssets instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameAssets>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
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

    public void test()
    {
    }

    public GameObject floatingTextObject;
    public GameObject tooltipObject;
    public Dictionary<string, Tile> tiles;
}


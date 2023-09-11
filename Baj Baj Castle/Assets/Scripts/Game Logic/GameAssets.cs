

// Class used to hold all game assets
namespace Game_Logic;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;
    public List<GameObject> EnemyPrefabs;

    public GameObject FloatingTextObject;
    public List<GameObject> ItemPrefabs;
    public List<GameObject> ObjectPrefabs;

    public GameObject PlayerPrefab;
    public Dictionary<string, Tile> Tiles;
    public List<GameObject> Triggers;

    public static GameAssets Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameAssets>();
                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }

    [UsedImplicitly]
    private void Awake()
    {
        var tileArray = Resources.LoadAll<Tile>("Art/Levels/Tiles/");
        Tiles = new Dictionary<string, Tile>();
        foreach (var tile in tileArray) Tiles[tile.name] = tile;
    }
}
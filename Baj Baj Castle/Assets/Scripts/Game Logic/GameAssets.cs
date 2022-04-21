using UnityEngine;

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

    public GameObject floatingTextObject;
}


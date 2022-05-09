using System;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        Menu,
        Game,
        GameOver,
        Loading
    }

    public static GameState LoadState;

    private static Action<GameState> onLoaderCallback;

    // Loads the specified scene and sets desired game state after loading
    public static void Load(Scene scene, GameState state)
    {
        LoadState = state;
        GameManager.Instance.GameState = GameState.Loading;

        // Sets the callback to be called after loading
        onLoaderCallback = gameState => { SceneManager.LoadScene(scene.ToString()); };

        // Loads loading scene
        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    // Callback function
    public static void LoaderCallback(GameState state)
    {
        if (onLoaderCallback != null)
        {
            GameManager.Instance.GameState = state;
            onLoaderCallback(state);
            onLoaderCallback = null;
        }
    }
}
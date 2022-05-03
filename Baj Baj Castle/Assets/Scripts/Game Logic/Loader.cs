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

    public static void Load(Scene scene, GameState state)
    {
        LoadState = state;
        GameManager.Instance.GameState = GameState.Loading;
        onLoaderCallback = gameState => { SceneManager.LoadScene(scene.ToString()); };

        SceneManager.LoadScene(Scene.Loading.ToString());
    }

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
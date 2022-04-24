using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public static GameState loadState;

    public enum Scene
    {
        Menu,
        Game,
        GameOver,
        Loading
    }

    private static Action<GameState> onLoaderCallback;

    public static void Load(Scene scene, GameState state)
    {
        loadState = state;
        GameManager.Instance.GameState = GameState.Loading;
        onLoaderCallback = (state) =>
        {
            SceneManager.LoadScene(scene.ToString());
        };

        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    public static void LoaderCallback(GameState state)
    {
        if (onLoaderCallback != null)
        {
            onLoaderCallback(state);
            GameManager.Instance.GameState = state;
            onLoaderCallback = null;
        }
    }
}

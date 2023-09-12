using System;
using Enums;
using UnityEngine.SceneManagement;

namespace Game_Logic
{
    public static class Loader
    {
        public enum Scene
        {
            Menu,
            Game,
            GameOver,
            Loading
        }

        public static GlobalGameState LoadState;

        private static Action<GlobalGameState> onLoaderCallback;

        // Loads the specified scene and sets desired game state after loading
        public static void Load(Scene scene, GlobalGameState state)
        {
            LoadState = state;
            GameManager.Instance.CurrentGlobalGameState = GlobalGameState.Loading;

            // Sets the callback to be called after loading
            onLoaderCallback = gameState => { SceneManager.LoadScene(scene.ToString()); };

            // Loads loading scene
            SceneManager.LoadScene(Scene.Loading.ToString());
        }

        // Callback function
        public static void LoaderCallback(GlobalGameState state)
        {
            if (onLoaderCallback != null)
            {
                GameManager.Instance.CurrentGlobalGameState = state;
                onLoaderCallback(state);
                onLoaderCallback = null;
            }
        }
    }
}
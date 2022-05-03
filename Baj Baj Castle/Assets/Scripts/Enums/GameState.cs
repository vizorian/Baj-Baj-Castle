using JetBrains.Annotations;

public enum GameState
{
    MainMenu,
    Escape,
    Tutorial,
    Pause,
    Loading,
    Victory,
    Defeat,
    Reload,
    [UsedImplicitly] None
}
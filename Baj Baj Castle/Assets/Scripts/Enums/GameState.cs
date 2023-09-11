using JetBrains.Annotations;

namespace Enums;

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
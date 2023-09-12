using JetBrains.Annotations;

namespace Enums
{
    public enum GlobalGameState
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
}
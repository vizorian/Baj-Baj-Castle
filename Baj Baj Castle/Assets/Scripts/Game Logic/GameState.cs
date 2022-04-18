public class GameState
{
    public static readonly GameState Playing = new GameState("Playing");
    public static readonly GameState Castle = new GameState("Castle");
    public static readonly GameState Paused = new GameState("Paused");
    public static readonly GameState GameOver = new GameState("GameOver");
    public static readonly GameState Menu = new GameState("Menu");
    private string name;

    private GameState(string name)
    {
        this.name = name;
    }

    public override string ToString()
    {
        return name;
    }
}
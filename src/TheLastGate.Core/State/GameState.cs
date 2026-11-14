namespace TheLastGate.State;

public sealed class GameState
{
    public PlayerState Player { get; } = new();
    public WorldMap Map { get; } = new();
}

namespace TheLastGate.State;

public sealed class GameState
{
    public PlayerState Player { get; } = new();
}

namespace TheLastGate.State;

public sealed class GameState
{
    public const int ServerTickCycle = 200;

    public PlayerState Player { get; } = new();
    public WorldMap Map { get; } = new();
    public TextLog Chat { get; } = new();
    public TextLog MessageOfTheDay { get; } = new();
    public Terminology Terminology { get; } = new();
    public int ServerTick { get; internal set; }
}

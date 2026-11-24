namespace TheLastGate.State;

public struct MapTile
{
    public int X { get; internal set; }
    public int Y { get; internal set; }

    public int BackgroundSprite { get; internal set; }
    public int Light { get; internal set; }
    public uint Flags { get; internal set; }
    public uint Flags2 { get; internal set; }

    public int CharacterSprite { get; internal set; }
    public int CharacterStatus { get; internal set; }
    public int CharacterStatusOffset { get; internal set; }
    public int CharacterSpeed { get; internal set; }
    public int CharacterNumber { get; internal set; }
    public int CharacterId { get; internal set; }
    public int CharacterHealthPercent { get; internal set; }

    // The item's tier (1-4), 5 for a grave holding items; plus 8 when a body holds items.
    public int Glow { get; internal set; }

    public int ItemSprite { get; internal set; }
    public int ItemStatus { get; internal set; }
}

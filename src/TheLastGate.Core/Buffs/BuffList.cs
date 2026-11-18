using System;

namespace TheLastGate.Buffs;

public static class BuffList
{
    public const string Header = "Your active buffs and debuffs:";
    public const int ProtectSprite = 86;
    public const int EnhanceSprite = 87;
    public const int BlessSprite = 88;

    public static BuffKind? SpriteKind(int sprite) => throw new NotImplementedException();

    public static bool IsHeader(string text) => throw new NotImplementedException();

    public static bool SlotNeedsFetch(int oldSprite, int oldActive, int newSprite, int newActive) =>
        throw new NotImplementedException();
}

public sealed class BuffListParser
{
    public BuffReading[] Readings { get; } = [new(), new(), new()];

    public bool Feed(string text) => throw new NotImplementedException();
}

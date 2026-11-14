using System;
using TheLastGate.State;

namespace TheLastGate.World;

public readonly record struct MagicGlow(bool Red, bool Green, bool Blue, int Strength)
{
    public const int Left = -32;
    public const int Top = -64;
    public const int Size = 64;

    public static MagicGlow? For(in MapTile tile) => throw new NotImplementedException();
}

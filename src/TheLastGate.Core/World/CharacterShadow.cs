using System;

namespace TheLastGate.World;

public readonly record struct CharacterShadow(int Left, int Top, int Height)
{
    public static bool Casts(int spriteNumber) => throw new NotImplementedException();

    public static CharacterShadow For(int width, int height) => throw new NotImplementedException();
}

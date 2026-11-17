using System;
using System.Collections.Generic;
using System.Numerics;

namespace TheLastGate.World;

public sealed class MovementGlide
{
    public static Vector2 WorldPoint(int x, int y, int offsetX, int offsetY) => throw new NotImplementedException();

    public void Record(
        Vector2 player,
        IReadOnlyDictionary<int, Vector2> characters,
        long nowMilliseconds,
        int ticks,
        double tickMilliseconds) =>
        throw new NotImplementedException();

    public Vector2 Player(long nowMilliseconds) => throw new NotImplementedException();

    public Vector2? Character(int characterNumber, long nowMilliseconds) => throw new NotImplementedException();
}

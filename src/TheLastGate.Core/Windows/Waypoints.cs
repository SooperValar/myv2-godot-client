using System;
using System.Collections.Generic;

namespace TheLastGate.Windows;

public readonly record struct Waypoint(int Number, string Name, string Description);

public static class Waypoints
{
    public const int Count = 27;

    public static IReadOnlyList<Waypoint> All => throw new NotImplementedException();

    public static bool Found(int foundMask, int number) => throw new NotImplementedException();
}

using System;

namespace TheLastGate.Map;

public enum MinimapButton
{
    ZoomIn,
    ZoomOut,
    Map,
}

public sealed class MinimapView
{
    public const int Pixels = 128;

    public int Magnification => throw new NotImplementedException();

    public static string Help(MinimapButton button) => throw new NotImplementedException();

    public void ZoomIn(bool toTheEnd) => throw new NotImplementedException();

    public void ZoomOut(bool toTheEnd) => throw new NotImplementedException();

    public void Scroll(int notches) => throw new NotImplementedException();

    public (int X, int Y) Origin(int playerX, int playerY) => throw new NotImplementedException();

    public void Render(ExploredMap map, int playerX, int playerY, Span<byte> rgba) =>
        throw new NotImplementedException();
}

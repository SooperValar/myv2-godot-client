using System;

namespace TheLastGate.Map;

public sealed class ExploredMap(int width = ExploredMap.Size, int height = ExploredMap.Size)
{
    public const int Size = 2048;

    public int Width => width;

    public int Height => height;

    public bool Changed => throw new NotImplementedException();

    public int Revision => throw new NotImplementedException();

    public static bool Delete(string path) => throw new NotImplementedException();

    public uint ColourAt(int tileX, int tileY) => throw new NotImplementedException();

    public void SetTile(int tileX, int tileY, uint colour) => throw new NotImplementedException();

    public void ClearBox(int left, int top, int boxWidth, int boxHeight) => throw new NotImplementedException();

    public void MarkSaved() => throw new NotImplementedException();

    public bool Load(ReadOnlySpan<byte> file) => throw new NotImplementedException();

    public byte[] Save() => throw new NotImplementedException();

    public bool LoadFrom(string path) => throw new NotImplementedException();

    public bool SaveTo(string path) => throw new NotImplementedException();
}

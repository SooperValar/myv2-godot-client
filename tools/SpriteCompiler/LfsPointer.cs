namespace TheLastGate.Tools.SpriteCompiler;

// When Git LFS objects haven't been downloaded, each data file is a small text pointer instead
// of the real file.
public static class LfsPointer
{
    private static ReadOnlySpan<byte> Start => "version https://git-lfs"u8;

    public static bool IsPointer(ReadOnlySpan<byte> fileStart) => fileStart.StartsWith(Start);
}

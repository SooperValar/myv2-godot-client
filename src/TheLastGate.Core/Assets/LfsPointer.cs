using System;

namespace TheLastGate.Assets;

// When Git LFS objects haven't been downloaded, each data file is a small text pointer instead
// of the real file.
public static class LfsPointer
{
    internal static int StartLength => Start.Length;

    private static ReadOnlySpan<byte> Start => "version https://git-lfs"u8;
    public static bool IsPointer(ReadOnlySpan<byte> fileStart) => fileStart.StartsWith(Start);

    public static string PulledMessage(string fileName) =>
        $"\"{fileName}\" is a Git LFS pointer, not the real file. Run git lfs pull.";
}

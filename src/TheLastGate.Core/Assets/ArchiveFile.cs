using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace TheLastGate.Assets;

// Reads from an archive data file by position, so one open handle serves every sprite.
internal sealed class ArchiveFile : IDisposable
{
    private readonly string fileName;
    private readonly SafeFileHandle handle;

    public ArchiveFile(string filePath)
    {
        handle = File.OpenHandle(filePath);
        fileName = Path.GetFileName(filePath);
        Length = RandomAccess.GetLength(handle);
    }

    public long Length { get; }

    public void Dispose() => handle.Dispose();

    public void ReadExactly(Span<byte> destination, long fileOffset)
    {
        if (fileOffset < 0 || fileOffset + destination.Length > Length)
        {
            throw new InvalidDataException(
                $"A read of {destination.Length} bytes at {fileOffset} is past the end of {fileName}.");
        }

        var totalRead = 0;
        while (totalRead < destination.Length)
        {
            var bytesRead = RandomAccess.Read(handle, destination[totalRead..], fileOffset + totalRead);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException($"{fileName} ended early while reading at {fileOffset}.");
            }

            totalRead += bytesRead;
        }
    }
}

using System.Buffers.Binary;
using SkiaSharp;
using TheLastGate.Assets;

namespace TheLastGate.Tools.SpriteCompiler;

// Rebuilds bmplib from the BMP sources and pnglib from the PNG sources. The archives are written
// as .tmp files first and only replace the real ones once all four are complete, so a failed
// compile leaves the previous archives in place.
public static class SpriteCompiler
{
    private const string TempSuffix = ".tmp";

    private static readonly string[] ArchiveFileNames =
    [
        DataLayout.BmpIndexFile, DataLayout.BmpDataFile, DataLayout.PngIndexFile, DataLayout.PngDataFile,
    ];

    public static CompileReport Compile(string sourceFolder, string archiveFolder)
    {
        var report = new CompileReport();

        try
        {
            var sources = SpriteSourceScanner.Scan(sourceFolder);
            report.Skipped.AddRange(sources.Skipped);
            report.InBothFormats.AddRange(sources.InBothFormats);
            if (sources.Bitmaps.Count == 0 && sources.Pngs.Count == 0)
            {
                throw new SpriteCompileException($"No source sprites were found in \"{sourceFolder}\".");
            }

            Directory.CreateDirectory(archiveFolder);
            WriteBmp(sources, archiveFolder, report);
            WritePngs(sources, archiveFolder, report);
            ThrowIfEverySourceRejected("BMP", "bmplib", sources.Bitmaps.Count, report.BmpSpriteCount);
            ThrowIfEverySourceRejected("PNG", "pnglib", sources.Pngs.Count, report.PngSpriteCount);
            ReplaceArchives(archiveFolder);
            report.Succeeded = true;
        }
        catch (Exception exception) when (exception is SpriteCompileException or IOException
            or UnauthorizedAccessException)
        {
            report.Error = exception.Message;
        }
        finally
        {
            foreach (var tempPath in ArchiveFileNames.Select(fileName => TempPath(archiveFolder, fileName))
                .Where(File.Exists))
            {
                File.Delete(tempPath);
            }
        }

        return report;
    }

    private static string TempPath(string archiveFolder, string fileName) =>
        Path.Combine(archiveFolder, fileName) + TempSuffix;

    private static void WriteBmp(SpriteSourceSet sources, string archiveFolder, CompileReport report)
    {
        using var indexStream = File.Create(TempPath(archiveFolder, DataLayout.BmpIndexFile));
        using var dataStream = File.Create(TempPath(archiveFolder, DataLayout.BmpDataFile));
        var writer = new BmpArchiveWriter(indexStream, dataStream);
        foreach (var (spriteNumber, filePath) in sources.Bitmaps)
        {
            var bitmap = BmpSpriteDecoder.Decode(ReadSource(filePath));
            if (bitmap.Pixels == null)
            {
                report.Skipped.Add(new SkippedFile(Path.GetFileName(filePath), bitmap.RejectionReason!));
                continue;
            }

            writer.Add(spriteNumber, bitmap.Width, bitmap.Height, bitmap.Pixels);
            report.BmpSpriteCount++;
        }

        writer.Finish();
        report.BmpDataBytes = writer.DataLength;
    }

    private static void WritePngs(SpriteSourceSet sources, string archiveFolder, CompileReport report)
    {
        using var indexStream = File.Create(TempPath(archiveFolder, DataLayout.PngIndexFile));
        using var dataStream = File.Create(TempPath(archiveFolder, DataLayout.PngDataFile));
        var writer = new PngArchiveWriter(indexStream, dataStream);
        foreach (var (spriteNumber, filePath) in sources.Pngs)
        {
            var pngBytes = ReadSource(filePath);
            var rejectionReason = CheckPng(pngBytes);
            if (rejectionReason != null)
            {
                report.Skipped.Add(new SkippedFile(Path.GetFileName(filePath), rejectionReason));
                continue;
            }

            writer.Add(spriteNumber, pngBytes);
            report.PngSpriteCount++;
        }

        writer.Finish();
        report.PngDataBytes = writer.DataLength;
    }

    private static void ThrowIfEverySourceRejected(
        string sourceKind,
        string archiveName,
        int sourceCount,
        int acceptedCount)
    {
        if (sourceCount > 0 && acceptedCount == 0)
        {
            throw new SpriteCompileException(
                $"Every {sourceKind} was rejected, so {archiveName} would be empty. See the skipped files above.");
        }
    }

    private static byte[] ReadSource(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        if (LfsPointer.IsPointer(fileBytes))
        {
            throw new SpriteCompileException(
                $"\"{Path.GetFileName(filePath)}\" is a Git LFS pointer, not an image. Run git lfs pull and try again.");
        }

        return fileBytes;
    }

    // PNGs are stored unchanged, so this checks the client can read them: Skia decodes the image,
    // and the chunk walk catches files cut off after their image data, which Skia accepts.
    private static string? CheckPng(byte[] pngBytes)
    {
        using var data = SKData.CreateCopy(pngBytes);
        using var codec = SKCodec.Create(data);
        if (codec?.EncodedFormat != SKEncodedImageFormat.Png)
        {
            return "not a PNG file";
        }

        if (!EndsWithCompleteEndChunk(pngBytes))
        {
            return "the file is cut off before the end of its IEND chunk";
        }

        var result = codec.GetPixels(codec.Info, out _);
        return result == SKCodecResult.Success ? null : $"can't be read as a PNG ({result})";
    }

    // Each chunk is a big-endian length, a four-letter type, the data and a four-byte CRC.
    private static bool EndsWithCompleteEndChunk(ReadOnlySpan<byte> pngBytes)
    {
        const int signatureSize = 8;
        const int lengthSize = 4;
        const int typeSize = 4;
        const int crcSize = 4;

        long position = signatureSize;
        while (position + lengthSize + typeSize <= pngBytes.Length)
        {
            var chunkHeader = pngBytes.Slice((int)position, lengthSize + typeSize);
            position += lengthSize + typeSize + BinaryPrimitives.ReadUInt32BigEndian(chunkHeader) + crcSize;
            if (chunkHeader[lengthSize..].SequenceEqual("IEND"u8))
            {
                return position <= pngBytes.Length;
            }
        }

        return false;
    }

    // Checks every archive can be replaced before replacing any, so a running client can't leave
    // a mix of old and new archives behind.
    private static void ReplaceArchives(string archiveFolder)
    {
        try
        {
            foreach (var finalPath in ArchiveFileNames.Select(fileName => Path.Combine(archiveFolder, fileName))
                .Where(File.Exists))
            {
                using var exclusive = new FileStream(finalPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }

            foreach (var fileName in ArchiveFileNames)
            {
                File.Move(TempPath(archiveFolder, fileName), Path.Combine(archiveFolder, fileName), true);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpriteCompileException(
                $"The archives couldn't be replaced ({exception.Message}). If the client is open, close the client and try again.");
        }
    }
}

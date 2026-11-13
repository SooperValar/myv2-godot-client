using System.Buffers.Binary;
using TheLastGate.Assets;
using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

public class BmpLibraryTests
{
    [Fact]
    public void Read_ReturnsRgbaPixelsWithMagentaTransparent()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteArchive(folder);

        using var library = new BmpLibrary(indexPath, dataPath);
        var sprite = library.Read(3)!;

        Assert.Equal(2, sprite.Width);
        Assert.Equal(1, sprite.Height);
        Assert.Equal(new byte[] { 255, 0, 0, 255, 255, 0, 255, 0 }, sprite.Rgba);
    }

    [Fact]
    public void Contains_IsTrueOnlyForStoredSprites()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteArchive(folder);

        using var library = new BmpLibrary(indexPath, dataPath);

        Assert.True(library.Contains(3));
        Assert.True(library.Contains(39999));
        Assert.False(library.Contains(4));
        Assert.False(library.Contains(-1));
        Assert.False(library.Contains(40000));
        Assert.Null(library.Read(4));
    }

    [Fact]
    public void Constructor_RejectsAShortIndex()
    {
        using var folder = new TempFolder();
        var indexPath = folder.Write("bmplib.idx", new byte[100]);
        var dataPath = folder.Write("bmplib.dat", []);

        Assert.Throws<InvalidDataException>(() => new BmpLibrary(indexPath, dataPath));
    }

    [Fact]
    public void Read_RejectsAnEntryPointingPastTheData()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteArchive(folder);
        var indexBytes = File.ReadAllBytes(indexPath);
        BinaryPrimitives.WriteInt32LittleEndian(indexBytes.AsSpan(3 * 12 + 8), 1000);
        File.WriteAllBytes(indexPath, indexBytes);

        using var library = new BmpLibrary(indexPath, dataPath);

        Assert.Throws<InvalidDataException>(() => library.Read(3));
    }

    private static (string IndexPath, string DataPath) WriteArchive(TempFolder folder)
    {
        var indexPath = Path.Combine(folder.FolderPath, "bmplib.idx");
        var dataPath = Path.Combine(folder.FolderPath, "bmplib.dat");
        using (var indexStream = File.Create(indexPath))
        using (var dataStream = File.Create(dataPath))
        {
            var writer = new BmpArchiveWriter(indexStream, dataStream);
            writer.Add(3, 2, 1, [0xF800, 0xF81F]);
            writer.Add(39999, 1, 1, [0x001F]);
            writer.Finish();
        }

        return (indexPath, dataPath);
    }
}

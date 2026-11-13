using TheLastGate.Assets;
using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

public class PngLibraryTests
{
    [Fact]
    public void ReadPng_ReturnsEachStoredFileUnchanged()
    {
        using var folder = new TempFolder();
        byte[][] pngs = [TestPngs.Create(1, 1, 7), TestPngs.Create(3, 2, 9), TestPngs.Create(2, 2, 11)];
        var (indexPath, dataPath) = WriteArchive(folder, (791, pngs[0]), (18001, pngs[1]), (133071, pngs[2]));

        using var library = new PngLibrary(indexPath, dataPath);

        Assert.Equal(pngs[0], library.ReadPng(791));
        Assert.Equal(pngs[1], library.ReadPng(18001));
        Assert.Equal(pngs[2], library.ReadPng(133071));
    }

    [Fact]
    public void Contains_IsTrueOnlyForStoredSprites()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteArchive(folder, (791, TestPngs.Create(1, 1, 7)));

        using var library = new PngLibrary(indexPath, dataPath);

        Assert.True(library.Contains(791));
        Assert.False(library.Contains(792));
        Assert.False(library.Contains(-1));
        Assert.False(library.Contains(SpriteNumbers.MaxSprite));
        Assert.Null(library.ReadPng(792));
    }

    [Fact]
    public void Constructor_RejectsAnOffsetPastTheData()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteArchive(folder, (791, TestPngs.Create(1, 1, 7)));
        var indexBytes = File.ReadAllBytes(indexPath);
        indexBytes[792 * 4] = 0xFF;
        indexBytes[792 * 4 + 1] = 0xFF;
        File.WriteAllBytes(indexPath, indexBytes);

        Assert.Throws<InvalidDataException>(() => new PngLibrary(indexPath, dataPath));
    }

    private static (string IndexPath, string DataPath) WriteArchive(
        TempFolder folder,
        params (int SpriteNumber, byte[] Png)[] sprites)
    {
        var indexPath = Path.Combine(folder.FolderPath, "pnglib.idx");
        var dataPath = Path.Combine(folder.FolderPath, "pnglib.dat");
        using (var indexStream = File.Create(indexPath))
        using (var dataStream = File.Create(dataPath))
        {
            var writer = new PngArchiveWriter(indexStream, dataStream);
            foreach (var (spriteNumber, png) in sprites)
            {
                writer.Add(spriteNumber, png);
            }

            writer.Finish();
        }

        return (indexPath, dataPath);
    }
}

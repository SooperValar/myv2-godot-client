using TheLastGate.Assets;
using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

public class ArchiveWriterTests
{
    [Fact]
    public void Bmp_IndexIsExactly480000Bytes()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteBmp(folder);

        Assert.Equal(480000, new FileInfo(indexPath).Length);
        Assert.Equal(6, new FileInfo(dataPath).Length);
    }

    [Fact]
    public void Bmp_WritesSizeAndOffsetPerSpriteAndPixelsInOrder()
    {
        using var folder = new TempFolder();
        var (indexPath, dataPath) = WriteBmp(folder);

        var indexBytes = File.ReadAllBytes(indexPath);
        Assert.Equal((2, 1, 0), TestArchives.BmpEntry(indexBytes, 3));
        Assert.Equal((1, 1, 4), TestArchives.BmpEntry(indexBytes, 39999));
        Assert.Equal((0, 0, 0), TestArchives.BmpEntry(indexBytes, 4));
        Assert.Equal(new byte[] { 0x00, 0xF8, 0x1F, 0xF8, 0x1F, 0x00 }, File.ReadAllBytes(dataPath));
    }

    [Fact]
    public void Bmp_RejectsOutOfOrderNumbers()
    {
        var writer = new BmpArchiveWriter(new MemoryStream(), new MemoryStream());
        writer.Add(5, 1, 1, [0]);

        Assert.Throws<ArgumentException>(() => writer.Add(5, 1, 1, [0]));
        Assert.Throws<ArgumentException>(() => writer.Add(4, 1, 1, [0]));
    }

    [Fact]
    public void Bmp_RejectsNumbersPastTheTable()
    {
        var writer = new BmpArchiveWriter(new MemoryStream(), new MemoryStream());

        Assert.Throws<ArgumentException>(() => writer.Add(40000, 1, 1, [0]));
    }

    [Fact]
    public void Bmp_RejectsPixelCountThatDoesNotMatchTheSize()
    {
        var writer = new BmpArchiveWriter(new MemoryStream(), new MemoryStream());

        Assert.Throws<ArgumentException>(() => writer.Add(1, 2, 2, [0, 0, 0]));
    }

    [Fact]
    public void Png_IndexIsExactly532288Bytes()
    {
        using var folder = new TempFolder();
        var (indexPath, _) = WritePngs(folder, TestPngs.Create(1, 1, 7), TestPngs.Create(2, 2, 9));

        Assert.Equal(532288, new FileInfo(indexPath).Length);
    }

    [Fact]
    public void Png_WritesScarThenEachPngUnchangedWithItsOffset()
    {
        using var folder = new TempFolder();
        var first = TestPngs.Create(1, 1, 7);
        var second = TestPngs.Create(3, 3, 9);
        var (indexPath, dataPath) = WritePngs(folder, first, second);

        var indexBytes = File.ReadAllBytes(indexPath);
        Assert.Equal(4, TestArchives.PngOffset(indexBytes, 791));
        Assert.Equal(4 + first.Length, TestArchives.PngOffset(indexBytes, 133071));
        Assert.Equal(0, TestArchives.PngOffset(indexBytes, 792));
        Assert.Equal([.. "SCAR"u8.ToArray(), .. first, .. second], File.ReadAllBytes(dataPath));
    }

    [Fact]
    public void Png_RejectsOutOfOrderNumbers()
    {
        var writer = new PngArchiveWriter(new MemoryStream(), new MemoryStream());
        writer.Add(10, TestPngs.Create(1, 1, 1));

        Assert.Throws<ArgumentException>(() => writer.Add(9, TestPngs.Create(1, 1, 1)));
        Assert.Throws<ArgumentException>(() => writer.Add(SpriteNumbers.MaxSprite, TestPngs.Create(1, 1, 1)));
    }

    private static (string IndexPath, string DataPath) WriteBmp(TempFolder folder)
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

    private static (string IndexPath, string DataPath) WritePngs(TempFolder folder, byte[] first, byte[] second)
    {
        var indexPath = Path.Combine(folder.FolderPath, "pnglib.idx");
        var dataPath = Path.Combine(folder.FolderPath, "pnglib.dat");
        using (var indexStream = File.Create(indexPath))
        using (var dataStream = File.Create(dataPath))
        {
            var writer = new PngArchiveWriter(indexStream, dataStream);
            writer.Add(791, first);
            writer.Add(133071, second);
            writer.Finish();
        }

        return (indexPath, dataPath);
    }
}

using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

public class SpriteSourceScannerTests
{
    private static readonly byte[] AnyBytes = [1, 2, 3];

    [Fact]
    public void Scan_ReadsNumbersAndExtensionsInAnyCase()
    {
        using var folder = new TempFolder();
        foreach (var name in new[] { "00012.bmp", "00013.BMP", "00791.png", "18001.PNG" })
        {
            folder.Write(name, AnyBytes);
        }

        var sources = SpriteSourceScanner.Scan(folder.FolderPath);

        Assert.Equal(new[] { 12, 13 }, sources.Bitmaps.Keys);
        Assert.Equal(new[] { 791, 18001 }, sources.Pngs.Keys);
        Assert.Equal(Path.Combine(folder.FolderPath, "00013.BMP"), sources.Bitmaps[13]);
        Assert.Empty(sources.Skipped);
    }

    [Fact]
    public void Scan_ListsNumbersInBothFormats()
    {
        using var folder = new TempFolder();
        folder.Write("00791.bmp", AnyBytes);
        folder.Write("00791.png", AnyBytes);
        folder.Write("00792.png", AnyBytes);

        var sources = SpriteSourceScanner.Scan(folder.FolderPath);

        Assert.Contains(791, sources.Bitmaps.Keys);
        Assert.Contains(791, sources.Pngs.Keys);
        Assert.Equal(new[] { 791 }, sources.InBothFormats);
    }

    [Fact]
    public void Scan_SkipsFilesThatAreNotSpriteNames()
    {
        using var folder = new TempFolder();
        string[] strays = ["Thumbs.db", "desktop.ini", "notes.txt", "791 copy.png", "abc.png", "1234567.png"];
        foreach (var name in strays)
        {
            folder.Write(name, AnyBytes);
        }

        folder.Write("nested/00001.png", AnyBytes);

        var sources = SpriteSourceScanner.Scan(folder.FolderPath);

        Assert.Empty(sources.Bitmaps);
        Assert.Empty(sources.Pngs);
        Assert.Equal(
            strays.OrderBy(name => name),
            sources.Skipped.Select(skipped => skipped.FileName).OrderBy(name => name));
    }

    [Fact]
    public void Scan_SkipsOutOfRangeNumbers()
    {
        using var folder = new TempFolder();
        folder.Write("40000.bmp", AnyBytes);
        folder.Write("133072.png", AnyBytes);
        folder.Write("39999.bmp", AnyBytes);
        folder.Write("133071.png", AnyBytes);

        var sources = SpriteSourceScanner.Scan(folder.FolderPath);

        Assert.Equal(new[] { 39999 }, sources.Bitmaps.Keys);
        Assert.Equal(new[] { 133071 }, sources.Pngs.Keys);
        Assert.Equal(2, sources.Skipped.Count);
        Assert.All(sources.Skipped, skipped => Assert.Contains("out of range", skipped.Reason));
    }

    [Fact]
    public void Scan_ThrowsOnSameNumberTwice()
    {
        using var folder = new TempFolder();
        folder.Write("5.png", AnyBytes);
        folder.Write("00005.png", AnyBytes);

        var exception = Assert.Throws<SpriteCompileException>(() => SpriteSourceScanner.Scan(folder.FolderPath));

        Assert.Contains("5.png", exception.Message);
        Assert.Contains("00005.png", exception.Message);
    }

    [Fact]
    public void Scan_ThrowsWhenFolderMissing()
    {
        var missing = Path.Combine(Path.GetTempPath(), "tlg-missing-" + Guid.NewGuid().ToString("N"));

        Assert.Throws<SpriteCompileException>(() => SpriteSourceScanner.Scan(missing));
    }
}

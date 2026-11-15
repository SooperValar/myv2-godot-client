using TheLastGate.Assets;

namespace TheLastGate.Tests;

public class SpriteOverridesTests
{
    [Fact]
    public void Find_MapsEachSpriteNumberToItsFile()
    {
        using var folder = new TempFolder();
        var framePath = folder.Write("00001.png", []);
        var darkFramePath = folder.Write("18001.bmp", []);
        var largePath = folder.Write("123456.png", []);

        var overrides = SpriteOverrides.Find(folder.FolderPath);

        Assert.Equal(3, overrides.Count);
        Assert.Equal(framePath, overrides[1]);
        Assert.Equal(darkFramePath, overrides[18001]);
        Assert.Equal(largePath, overrides[123456]);
    }

    [Fact]
    public void Find_PrefersThePngWhenBothFilesExist()
    {
        using var folder = new TempFolder();
        folder.Write("00001.bmp", []);
        var pngPath = folder.Write("00001.png", []);
        folder.Write("00002.png", []);
        folder.Write("00002.bmp", []);

        var overrides = SpriteOverrides.Find(folder.FolderPath);

        Assert.Equal(pngPath, overrides[1]);
        Assert.EndsWith("00002.png", overrides[2]);
    }

    [Fact]
    public void Find_AcceptsExtensionsInAnyCase()
    {
        using var folder = new TempFolder();
        var framePath = folder.Write("00001.PNG", []);
        var darkFramePath = folder.Write("18001.Bmp", []);
        folder.Write("00002.BMP", []);
        var pngPath = folder.Write("00002.Png", []);

        var overrides = SpriteOverrides.Find(folder.FolderPath);

        Assert.Equal(framePath, overrides[1]);
        Assert.Equal(darkFramePath, overrides[18001]);
        Assert.Equal(pngPath, overrides[2]);
    }

    [Theory]
    [InlineData("1.png")]
    [InlineData("0001.png")]
    [InlineData("000001.png")]
    [InlineData("frame.png")]
    [InlineData("00001.jpg")]
    [InlineData("00001.png.txt")]
    [InlineData("-0001.png")]
    public void Find_IgnoresFilesTheCurrentClientWouldNeverOpen(string fileName)
    {
        using var folder = new TempFolder();
        folder.Write(fileName, []);

        Assert.Empty(SpriteOverrides.Find(folder.FolderPath));
    }

    [Fact]
    public void Find_IgnoresNumbersPastTheLastSprite()
    {
        using var folder = new TempFolder();
        folder.Write($"{SpriteNumbers.MaxSprite}.png", []);

        Assert.Empty(SpriteOverrides.Find(folder.FolderPath));
    }

    [Fact]
    public void Find_ReturnsNothingWhenTheFolderIsMissing()
    {
        using var folder = new TempFolder();

        Assert.Empty(SpriteOverrides.Find(Path.Combine(folder.FolderPath, "overrides")));
    }
}

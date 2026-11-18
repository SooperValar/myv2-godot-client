using System.Text;
using TheLastGate.Assets;

namespace TheLastGate.Tests;

public class LfsPointerTests
{
    private static readonly byte[] Pointer =
        Encoding.ASCII.GetBytes("version https://git-lfs.github.com/spec/v1\noid sha256:ab\nsize 480000\n");

    [Fact]
    public void BmpLibrary_ExplainsAnUnpulledIndex()
    {
        using var folder = new TempFolder();
        var indexPath = folder.Write("bmplib.idx", Pointer);
        var dataPath = folder.Write("bmplib.dat", Pointer);

        var exception = Assert.Throws<InvalidDataException>(() => new BmpLibrary(indexPath, dataPath));

        Assert.Contains("git lfs pull", exception.Message);
    }

    [Fact]
    public void PngLibrary_ExplainsAnUnpulledIndex()
    {
        using var folder = new TempFolder();
        var indexPath = folder.Write("pnglib.idx", Pointer);
        var dataPath = folder.Write("pnglib.dat", Pointer);

        var exception = Assert.Throws<InvalidDataException>(() => new PngLibrary(indexPath, dataPath));

        Assert.Contains("git lfs pull", exception.Message);
    }

    [Fact]
    public void PngLibrary_ExplainsAnUnpulledDataFile()
    {
        using var folder = new TempFolder();
        var indexPath = folder.Write("pnglib.idx", new byte[SpriteNumbers.MaxSprite * 4]);
        var dataPath = folder.Write("pnglib.dat", Pointer);

        var exception = Assert.Throws<InvalidDataException>(() => new PngLibrary(indexPath, dataPath));

        Assert.Contains("git lfs pull", exception.Message);
    }
}

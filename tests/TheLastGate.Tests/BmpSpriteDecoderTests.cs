using System.Text;
using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

public class BmpSpriteDecoderTests
{
    private static readonly (byte, byte, byte) Red = (255, 0, 0);
    private static readonly (byte, byte, byte) Blue = (0, 0, 255);

    [Fact]
    public void Decode_TruncatesColoursToRgb565()
    {
        (byte, byte, byte)[] colours = [(255, 0, 0), (0, 255, 0), (0, 0, 255), (255, 0, 255), (15, 7, 15)];
        var bitmap = TestBitmaps.Create(colours.Length, 1, (column, _) => colours[column]);

        var result = BmpSpriteDecoder.Decode(bitmap);

        Assert.Null(result.RejectionReason);
        Assert.Equal(new ushort[] { 0xF800, 0x07E0, 0x001F, 0xF81F, 0x0821 }, result.Pixels);
    }

    [Fact]
    public void Decode_ReturnsRowsTopDown()
    {
        var bitmap = TestBitmaps.Create(1, 2, (_, row) => row == 0 ? Red : Blue);

        var result = BmpSpriteDecoder.Decode(bitmap);

        Assert.Equal(1, result.Width);
        Assert.Equal(2, result.Height);
        Assert.Equal(new ushort[] { 0xF800, 0x001F }, result.Pixels);
    }

    [Fact]
    public void Decode_Accepts8BitPaletteBitmaps()
    {
        var bitmap = TestBitmaps.Create(2, 1, (column, _) => column == 0 ? Red : Blue, 8);

        var result = BmpSpriteDecoder.Decode(bitmap);

        Assert.Null(result.RejectionReason);
        Assert.Equal(new ushort[] { 0xF800, 0x001F }, result.Pixels);
    }

    [Fact]
    public void Decode_RejectsOversize()
    {
        var bitmap = TestBitmaps.Create(1001, 1, (_, _) => Red);

        var result = BmpSpriteDecoder.Decode(bitmap);

        Assert.Null(result.Pixels);
        Assert.Equal("size 1001x1 is larger than 1000x1000", result.RejectionReason);
    }

    [Fact]
    public void Decode_AcceptsLargestSize()
    {
        var bitmap = TestBitmaps.Create(1000, 1000, (_, _) => Red);

        Assert.NotNull(BmpSpriteDecoder.Decode(bitmap).Pixels);
    }

    [Fact]
    public void Decode_RejectsTruncatedFile()
    {
        var bitmap = TestBitmaps.Create(3, 3, (_, _) => Red);

        var result = BmpSpriteDecoder.Decode(bitmap[..^10]);

        Assert.Null(result.Pixels);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public void Decode_RejectsTextFile()
    {
        var pointer = Encoding.ASCII.GetBytes("version https://git-lfs.github.com/spec/v1\noid sha256:00\nsize 12\n");

        var result = BmpSpriteDecoder.Decode(pointer);

        Assert.Null(result.Pixels);
        Assert.Equal("not a BMP file", result.RejectionReason);
    }

    [Fact]
    public void Decode_RejectsPngSavedWithBmpName()
    {
        var result = BmpSpriteDecoder.Decode(TestPngs.Create(1, 1, 1));

        Assert.Null(result.Pixels);
        Assert.Equal("not a BMP file", result.RejectionReason);
    }
}

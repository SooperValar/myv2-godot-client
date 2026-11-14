using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TickDecompressorTests
{
    [Fact]
    public void Decompress_ReturnsEachTickOfOneSessionStreamInOrder()
    {
        var repeatedTick = Enumerable.Range(0, 300).Select(value => (byte)(value % 17)).ToArray();
        byte[] shortTick = [1, 2, 3];
        using var compressor = new TestTickCompressor();
        var frames = new[] { repeatedTick, repeatedTick, shortTick }.Select(compressor.Compress).ToArray();
        using var decompressor = new TickDecompressor();

        Assert.Equal(repeatedTick, Decompress(decompressor, frames[0]));
        Assert.Equal(repeatedTick, Decompress(decompressor, frames[1]));
        Assert.Equal(shortTick, Decompress(decompressor, frames[2]));
    }

    [Fact]
    public void Decompress_ReturnsATickOfExactlyTheMaximumSize()
    {
        var largestTick = RandomBytes(TickDecompressor.MaxPayloadSize);
        using var compressor = new TestTickCompressor();
        using var decompressor = new TickDecompressor();

        Assert.Equal(largestTick, Decompress(decompressor, compressor.Compress(largestTick)));
    }

    [Fact]
    public void Decompress_RejectsATickOverTheMaximumSize()
    {
        using var compressor = new TestTickCompressor();
        var frame = compressor.Compress(RandomBytes(TickDecompressor.MaxPayloadSize + 1));
        using var decompressor = new TickDecompressor();

        Assert.Throws<InvalidDataException>(() => Decompress(decompressor, frame));
    }

    [Fact]
    public void Decompress_RejectsDataThatIsNotZlib()
    {
        byte[] frame = [0x78, 0x9C, 0xFF, 0xFF, 0xFF, 0xFF];
        using var decompressor = new TickDecompressor();

        Assert.Throws<InvalidDataException>(() => Decompress(decompressor, frame));
    }

    [Fact]
    public void Decompress_RejectsAnOutputSmallerThanTheMaximumSize()
    {
        using var compressor = new TestTickCompressor();
        var frame = compressor.Compress([1, 2, 3]);
        using var decompressor = new TickDecompressor();

        Assert.Throws<ArgumentException>(() =>
            decompressor.Decompress(frame, new byte[TickDecompressor.MaxPayloadSize - 1]));
    }

    private static byte[] Decompress(TickDecompressor decompressor, byte[] frame)
    {
        var output = new byte[TickDecompressor.MaxPayloadSize];
        var length = decompressor.Decompress(frame, output);
        return output[..length];
    }

    private static byte[] RandomBytes(int count)
    {
        var bytes = new byte[count];
        new Random(count).NextBytes(bytes);
        return bytes;
    }
}

using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TickFrameReaderTests
{
    private const int CompressedFlag = 0x8000;

    [Fact]
    public async Task ReadAsync_ReturnsTheBodyOfAnUncompressedFrame()
    {
        var reader = new TickFrameReader(new MemoryStream(Frame(false, 7, 8, 9)));

        var frame = await reader.ReadAsync(CancellationToken.None);

        Assert.NotNull(frame);
        Assert.False(frame.IsCompressed);
        Assert.Equal(new byte[] { 7, 8, 9 }, frame.Body);
    }

    [Fact]
    public async Task ReadAsync_FlagsACompressedFrame()
    {
        var reader = new TickFrameReader(new MemoryStream(Frame(true, 7, 8, 9)));

        var frame = await reader.ReadAsync(CancellationToken.None);

        Assert.NotNull(frame);
        Assert.True(frame.IsCompressed);
        Assert.Equal(new byte[] { 7, 8, 9 }, frame.Body);
    }

    [Fact]
    public async Task ReadAsync_ReadsFramesInOrderThenNullAtTheEnd()
    {
        var reader = new TickFrameReader(new MemoryStream([.. Frame(false, 1), .. Frame(true, 2, 3), .. Frame(false)]));

        var first = await reader.ReadAsync(CancellationToken.None);
        var second = await reader.ReadAsync(CancellationToken.None);
        var third = await reader.ReadAsync(CancellationToken.None);
        var end = await reader.ReadAsync(CancellationToken.None);

        Assert.Equal(new byte[] { 1 }, first?.Body);
        Assert.Equal(new byte[] { 2, 3 }, second?.Body);
        Assert.Equal(Array.Empty<byte>(), third?.Body);
        Assert.Null(end);
    }

    [Fact]
    public async Task ReadAsync_JoinsAFrameThatArrivesOneByteAtATime()
    {
        var reader = new TickFrameReader(new OneByteAtATimeStream(Frame(true, 4, 5, 6, 7)));

        var frame = await reader.ReadAsync(CancellationToken.None);

        Assert.Equal(new byte[] { 4, 5, 6, 7 }, frame?.Body);
    }

    [Fact]
    public async Task ReadAsync_ReadsTheLargestFrameTheHeaderAllows()
    {
        var body = Enumerable.Range(0, 0x7FFF - 2).Select(value => (byte)value).ToArray();
        var reader = new TickFrameReader(new MemoryStream(Frame(false, body)));

        var frame = await reader.ReadAsync(CancellationToken.None);

        Assert.Equal(body, frame?.Body);
    }

    [Fact]
    public async Task ReadAsync_RejectsALengthShorterThanTheHeader()
    {
        var reader = new TickFrameReader(new MemoryStream([1, 0]));

        await Assert.ThrowsAsync<ProtocolDesyncException>(() => reader.ReadAsync(CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task ReadAsync_ThrowsWhenTheStreamEndsInsideAFrame()
    {
        var reader = new TickFrameReader(new MemoryStream(Frame(false, 1, 2, 3, 4)[..^1]));

        await Assert.ThrowsAsync<EndOfStreamException>(() => reader.ReadAsync(CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task ReadAsync_ThrowsWhenTheStreamEndsInsideAHeader()
    {
        var reader = new TickFrameReader(new MemoryStream([5]));

        await Assert.ThrowsAsync<EndOfStreamException>(() => reader.ReadAsync(CancellationToken.None).AsTask());
    }

    private static byte[] Frame(bool compressed, params byte[] body)
    {
        var header = (body.Length + 2) | (compressed ? CompressedFlag : 0);
        return [(byte)header, (byte)(header >> 8), .. body];
    }

    private sealed class OneByteAtATimeStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            base.ReadAsync(buffer[..Math.Min(1, buffer.Length)], cancellationToken);
    }
}

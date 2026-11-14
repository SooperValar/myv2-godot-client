using System;
using System.IO;
using System.IO.Compression;

namespace TheLastGate.Net;

public sealed class TickDecompressor : IDisposable
{
    public const int MaxPayloadSize = 65536;

    private readonly FrameInput input = new();
    private readonly ZLibStream zlib;

    public TickDecompressor() => zlib = new ZLibStream(input, CompressionMode.Decompress);

    public void Dispose() => zlib.Dispose();

    public int Decompress(ReadOnlyMemory<byte> compressedFrame, Span<byte> output)
    {
        if (output.Length < MaxPayloadSize)
        {
            throw new ArgumentException($"The output must hold {MaxPayloadSize} bytes.", nameof(output));
        }

        input.Frame = compressedFrame;
        var totalRead = 0;
        while (totalRead < MaxPayloadSize)
        {
            var bytesRead = zlib.Read(output[totalRead..MaxPayloadSize]);
            if (bytesRead == 0)
            {
                return totalRead;
            }

            totalRead += bytesRead;
        }

        Span<byte> overflowProbe = stackalloc byte[1];
        if (zlib.Read(overflowProbe) > 0)
        {
            throw new InvalidDataException($"A tick decompressed to more than {MaxPayloadSize} bytes.");
        }

        return totalRead;
    }

    // ZLibStream takes a read of 0 bytes as "no input yet" rather than the end of the stream, so it
    // carries on from where the last frame stopped when the next frame arrives.
    private sealed class FrameInput : Stream
    {
        public ReadOnlyMemory<byte> Frame { get; set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            var byteCount = Math.Min(buffer.Length, Frame.Length);
            Frame.Span[..byteCount].CopyTo(buffer);
            Frame = Frame[byteCount..];
            return byteCount;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

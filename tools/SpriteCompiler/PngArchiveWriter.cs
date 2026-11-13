using System.Buffers.Binary;
using TheLastGate.Assets;

namespace TheLastGate.Tools.SpriteCompiler;

// Writes pnglib: "SCAR", then each PNG file's bytes unchanged. The index of offsets is written on
// Finish, with 0 meaning the sprite is absent.
public sealed class PngArchiveWriter
{
    private readonly Stream dataStream;
    private readonly Stream indexStream;
    private readonly int[] offsets = new int[SpriteNumbers.MaxSprite];
    private int lastSpriteNumber = -1;

    public PngArchiveWriter(Stream indexStream, Stream dataStream)
    {
        this.indexStream = indexStream;
        this.dataStream = dataStream;
        dataStream.Write(Magic);
        DataLength = Magic.Length;
    }

    private static ReadOnlySpan<byte> Magic => "SCAR"u8;

    public long DataLength { get; private set; }

    public void Add(int spriteNumber, ReadOnlySpan<byte> pngBytes)
    {
        if (spriteNumber <= lastSpriteNumber || spriteNumber >= SpriteNumbers.MaxSprite)
        {
            throw new ArgumentException(
                $"Sprite {spriteNumber} must be above {lastSpriteNumber} and below {SpriteNumbers.MaxSprite}.",
                nameof(spriteNumber));
        }

        if (DataLength + pngBytes.Length > int.MaxValue)
        {
            throw new SpriteCompileException($"pnglib.dat would pass {int.MaxValue} bytes at sprite {spriteNumber}.");
        }

        offsets[spriteNumber] = (int)DataLength;
        dataStream.Write(pngBytes);
        DataLength += pngBytes.Length;
        lastSpriteNumber = spriteNumber;
    }

    public void Finish()
    {
        var indexBytes = new byte[offsets.Length * sizeof(int)];
        for (var slot = 0; slot < offsets.Length; slot++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(indexBytes.AsSpan(slot * sizeof(int)), offsets[slot]);
        }

        indexStream.Write(indexBytes);
    }
}

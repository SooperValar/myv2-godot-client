using TheLastGate.Net;

namespace TheLastGate.Tests;

public class PacketReaderTests
{
    [Fact]
    public void Reads_ValuesLittleEndianInOrder()
    {
        byte[] packet = [0x2A, 0x34, 0x12, 0xFE, 0xFF, 0x78, 0x56, 0x34, 0x12];
        var reader = new PacketReader(packet);

        Assert.Equal(0x2A, reader.ReadByte());
        Assert.Equal(0x1234, reader.ReadUInt16());
        Assert.Equal(-2, reader.ReadInt16());
        Assert.Equal(0x12345678u, reader.ReadUInt32());
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void ReadBytes_ReturnsTheNextBytesAndMovesOn()
    {
        byte[] packet = [1, 2, 3, 4, 5];
        var reader = new PacketReader(packet);
        reader.Skip(1);

        var taken = reader.ReadBytes(3).ToArray();

        Assert.Equal(new byte[] { 2, 3, 4 }, taken);
        Assert.Equal(4, reader.Position);
        Assert.Equal(1, reader.Remaining);
    }

    [Fact]
    public void ReadingPastTheEnd_ThrowsADesyncNamingTheOffset()
    {
        byte[] packet = [1, 2, 3];

        var exception = Assert.Throws<ProtocolDesyncException>(() =>
        {
            var reader = new PacketReader(packet);
            reader.Skip(2);
            reader.ReadUInt16();
        });

        Assert.Contains("2 bytes at offset 2", exception.Message);
        Assert.Contains("1 are left", exception.Message);
    }

    [Fact]
    public void ReadBytes_RejectsANegativeCount()
    {
        byte[] packet = [1, 2, 3];

        Assert.Throws<ProtocolDesyncException>(() => new PacketReader(packet).ReadBytes(-1));
    }
}

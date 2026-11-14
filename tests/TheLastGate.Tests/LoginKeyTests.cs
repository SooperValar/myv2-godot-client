using System.Buffers.Binary;
using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class LoginKeyTests
{
    [Fact]
    public void Parse_ReadsEachFieldLittleEndian()
    {
        var keyBytes = new byte[LoginKey.Size];
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes, 1234);
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes.AsSpan(4), 0xDEADBEEF);
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes.AsSpan(8), 7);
        "Ishtar"u8.CopyTo(keyBytes.AsSpan(12));
        BinaryPrimitives.WriteInt32LittleEndian(keyBytes.AsSpan(52), 110009);

        var key = LoginKey.Parse(keyBytes);

        Assert.Equal(new LoginKey(1234, 0xDEADBEEF, 7, "Ishtar", 110009), key);
    }

    [Fact]
    public void Write_ThenParse_GivesTheSameKey()
    {
        var key = new LoginKey(99, 1, 2, "Élan", 6);
        var keyBytes = new byte[LoginKey.Size];

        key.Write(keyBytes);

        Assert.Equal(key, LoginKey.Parse(keyBytes));
    }

    [Fact]
    public void Write_TruncatesTheNameToFitItsField()
    {
        var keyBytes = new byte[LoginKey.Size];

        new LoginKey(0, 0, 0, new string('x', 50), 0).Write(keyBytes);

        Assert.Equal(new string('x', 39), LoginKey.Parse(keyBytes).Name);
    }
}

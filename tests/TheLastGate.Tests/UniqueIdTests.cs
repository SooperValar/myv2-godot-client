using TheLastGate.Net;

namespace TheLastGate.Tests;

public class UniqueIdTests
{
    [Fact]
    public void Parse_ReadsTheLowThenTheHighHalfLittleEndian()
    {
        byte[] bytes = [0x04, 0x03, 0x02, 0x01, 0x08, 0x07, 0x06, 0x05];

        Assert.Equal(new UniqueId(0x01020304, 0x05060708), UniqueId.Parse(bytes));
    }

    [Fact]
    public void Parse_GivesNothingForFewerThanEightBytes()
    {
        Assert.Null(UniqueId.Parse(new byte[UniqueId.Size - 1]));
    }

    [Fact]
    public void ToBytes_WritesWhatParseReads()
    {
        var uniqueId = new UniqueId(0xDEADBEEF, 0x12345678);

        Assert.Equal(uniqueId, UniqueId.Parse(uniqueId.ToBytes()));
        Assert.Equal(UniqueId.Size, uniqueId.ToBytes().Length);
    }

    [Fact]
    public void CreateRandom_NeverGivesAZeroHalf()
    {
        Assert.Equal(new UniqueId(1, 1), UniqueId.CreateRandom(new LowestRandom()));
    }

    [Fact]
    public void CreateRandom_UsesTheWholeRangeOfEachHalf()
    {
        Assert.Equal(new UniqueId(uint.MaxValue, uint.MaxValue), UniqueId.CreateRandom(new HighestRandom()));
    }

    private sealed class LowestRandom : Random
    {
        public override long NextInt64(long minValue, long maxValue) => minValue;
    }

    private sealed class HighestRandom : Random
    {
        public override long NextInt64(long minValue, long maxValue) => maxValue - 1;
    }
}

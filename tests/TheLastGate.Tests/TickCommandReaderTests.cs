using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TickCommandReaderTests
{
    [Fact]
    public void TryReadNext_ReturnsEachCommandAtItsOwnLength()
    {
        byte[] relativeSetMap = [0x80 | 1, 0b0000_0001, 0x34, 0x12];
        var log = new byte[16];
        log[0] = (byte)ServerCommand.Log0;
        byte[] payload = [(byte)ServerCommand.Tick, 7, .. relativeSetMap, .. log];
        var reader = new TickCommandReader(payload);

        Assert.True(reader.TryReadNext(out var first));
        Assert.Equal(new byte[] { (byte)ServerCommand.Tick, 7 }, first.ToArray());
        Assert.True(reader.TryReadNext(out var second));
        Assert.Equal(relativeSetMap, second.ToArray());
        Assert.True(reader.TryReadNext(out var third));
        Assert.Equal(log, third.ToArray());
        Assert.False(reader.TryReadNext(out _));
    }

    [Fact]
    public void TryReadNext_FindsNothingInAnEmptyTick()
    {
        var reader = new TickCommandReader([]);

        Assert.False(reader.TryReadNext(out _));
    }

    [Fact]
    public void TryReadNext_RejectsACommandLongerThanWhatIsLeft()
    {
        byte[] payload = [(byte)ServerCommand.Tick, 7, (byte)ServerCommand.SetCharacterGold, 1, 2];

        var exception = Assert.Throws<ProtocolDesyncException>(() =>
        {
            var reader = new TickCommandReader(payload);
            while (reader.TryReadNext(out _))
            {
            }
        });

        Assert.Contains("command 2", exception.Message);
        Assert.Contains("offset 2", exception.Message);
    }

    [Fact]
    public void TryReadNext_NamesTheCommandAndOffsetOfAnUnknownCommand()
    {
        byte[] payload = [(byte)ServerCommand.Tick, 7, (byte)ServerCommand.ScrollRight, 26];

        var exception = Assert.Throws<ProtocolDesyncException>(() =>
        {
            var reader = new TickCommandReader(payload);
            while (reader.TryReadNext(out _))
            {
            }
        });

        Assert.Contains("command 3 (id 26) at offset 3 of 4", exception.Message);
    }
}

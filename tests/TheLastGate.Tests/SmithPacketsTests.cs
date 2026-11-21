using TheLastGate.Net;

namespace TheLastGate.Tests;

public class SmithPacketsTests
{
    private const int SmithNumber = 1302;

    [Fact]
    public void Place_SendsTheSmithAndTheSlot()
    {
        byte[] expected = [34, 22, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SmithPackets.Place(SmithNumber, 2));
    }

    [Fact]
    public void TakeBack_SendsTheSlotPast16()
    {
        byte[] expected = [34, 22, 5, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SmithPackets.TakeBack(SmithNumber, 2));
    }

    [Fact]
    public void Press_SendsTheButton()
    {
        byte[] expected = [34, 22, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SmithPackets.Press(SmithNumber, 5));
    }

    [Theory]
    [InlineData(2, 10)]
    [InlineData(6, 14)]
    public void Check_SendsTheSlotOrButtonPast8(int slotOrButton, byte value)
    {
        byte[] expected = [34, 22, 5, value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SmithPackets.Check(SmithNumber, slotOrButton));
    }
}

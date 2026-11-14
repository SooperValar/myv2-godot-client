using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TickReportTests
{
    [Theory]
    [InlineData(0u, false)]
    [InlineData(15u, false)]
    [InlineData(16u, true)]
    [InlineData(17u, false)]
    [InlineData(32u, true)]
    public void IsDue_EverySixteenTicksPlayed(uint ticksPlayed, bool expected)
    {
        Assert.Equal(expected, TickReport.IsDue(ticksPlayed));
    }

    [Fact]
    public void Create_SendsTheCountOfTicksPlayed()
    {
        var report = TickReport.Create(0x01020304);

        Assert.Equal(Protocol.PacketSize, report.Length);
        Assert.Equal((byte)ClientCommand.TickReport, report[0]);
        Assert.Equal(new byte[] { 0x04, 0x03, 0x02, 0x01 }, report[1..5]);
        Assert.All(report[5..], value => Assert.Equal(0, value));
    }
}

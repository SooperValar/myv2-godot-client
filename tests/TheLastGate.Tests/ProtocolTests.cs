using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ProtocolTests
{
    [Theory]
    [InlineData(0x000F00u, "0.15.00")]
    [InlineData(0x0A0B0Cu, "10.11.12")]
    [InlineData(0u, "0.00.00")]
    public void VersionText_SplitsTheVersionIntoItsThreeBytes(uint version, string text)
    {
        Assert.Equal(text, Protocol.VersionText(version));
    }
}

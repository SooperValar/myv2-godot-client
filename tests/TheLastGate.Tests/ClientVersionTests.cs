using TheLastGate.Hud;

namespace TheLastGate.Tests;

public class ClientVersionTests
{
    [Theory]
    [InlineData("0.0.0")]
    [InlineData("0.0.0+5e0c1d7")]
    [InlineData("")]
    public void Text_CallsEveryBuildBeforeTheFirstReleaseAReleaseCandidate(string version)
    {
        Assert.Equal("Client Version RC", ClientVersion.Text(version));
    }

    [Theory]
    [InlineData("1.0.0", "Client Version 1.0.0")]
    [InlineData("1.4.2+5e0c1d7", "Client Version 1.4.2")]
    [InlineData("0.1.0", "Client Version 0.1.0")]
    public void Text_ShowsTheReleasedVersionWithoutTheCommit(string version, string text)
    {
        Assert.Equal(text, ClientVersion.Text(version));
    }
}

using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class ClientVersionTests
{
    [TestCase("0.0.0")]
    [TestCase("0.0.0+5e0c1d7")]
    [TestCase("")]
    public void Text_CallsEveryBuildBeforeTheFirstReleaseAReleaseCandidate(string version)
    {
        AssertThat(ClientVersion.Text(version)).IsEqual("Client Version RC");
    }

    [TestCase("1.0.0", "Client Version 1.0.0")]
    [TestCase("1.4.2+5e0c1d7", "Client Version 1.4.2")]
    [TestCase("0.1.0", "Client Version 0.1.0")]
    public void Text_ShowsTheReleasedVersionWithoutTheCommit(string version, string text)
    {
        AssertThat(ClientVersion.Text(version)).IsEqual(text);
    }
}

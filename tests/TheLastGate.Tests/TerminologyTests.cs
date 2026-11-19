using TheLastGate.State;

namespace TheLastGate.Tests;

public class TerminologyTests
{
    [Fact]
    public void AskForMetaText_AsksOnlyTheFirstTime()
    {
        var terminology = new Terminology();

        Assert.True(terminology.AskForMetaText());
        Assert.False(terminology.AskForMetaText());
    }
}

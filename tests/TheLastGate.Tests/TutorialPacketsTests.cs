using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TutorialPacketsTests
{
    [Fact]
    public void Seen_SendsTheTutorial()
    {
        byte[] expected = [36, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, TutorialPackets.Seen(3));
    }

    [Fact]
    public void SkipTutorials_Sends16()
    {
        byte[] expected = [36, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, TutorialPackets.SkipTutorials());
    }
}

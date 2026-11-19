using TheLastGate.Assets;
using TheLastGate.State;
using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class MetaLinesTests
{
    [Theory]
    [InlineData(1234, false, "1234")]
    [InlineData(0, false, "0")]
    [InlineData(1234, true, "12.34")]
    [InlineData(1205, true, "12.05")]
    [InlineData(7, true, "0.07")]
    public void Value_PrintsWholeNumbersOrHundredths(int value, bool hundredths, string text)
    {
        Assert.Equal(text, MetaLines.Value(value, hundredths));
    }

    [Fact]
    public void Value_LeavesTheUnsetValueBlank()
    {
        Assert.Equal("", MetaLines.Value(65535, false));
    }

    [Theory]
    [InlineData(0, TextColour.Red)]
    [InlineData(1, TextColour.Yellow)]
    [InlineData(7, TextColour.Teal)]
    [InlineData(9, TextColour.White)]
    [InlineData(12, TextColour.Yellow)]
    [InlineData(-1, TextColour.Yellow)]
    public void Colour_FollowsTheServersFontNumbers(int font, TextColour colour)
    {
        Assert.Equal(colour, MetaLines.Colour(font));
    }

    [Fact]
    public void Description_PrintsTheNameThenTheText()
    {
        Assert.Equal(
            [
                new LogLine("Hit Score", (int)TextColour.Orange),
                new LogLine("How often you hit.", (int)TextColour.Yellow),
            ],
            MetaLines.Description("Hit Score", "How often you hit."));
    }

    [Theory]
    [InlineData("  Passive Stats:")]
    [InlineData("  Active Stats:")]
    public void Description_OfAHeading_IsEmpty(string heading)
    {
        Assert.Empty(MetaLines.Description(heading, ""));
    }
}

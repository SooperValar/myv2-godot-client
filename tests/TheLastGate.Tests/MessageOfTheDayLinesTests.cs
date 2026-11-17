using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class MessageOfTheDayLinesTests
{
    private const string LongSentence = "The quick brown fox jumps over the lazy dog and keeps running far away";

    private readonly MessageOfTheDayLines lines = new();

    [Fact]
    public void Lines_StartEmpty()
    {
        Assert.Equal(MessageOfTheDayLines.VisibleLines, lines.Lines.Count);
        Assert.All(lines.Lines, line => Assert.Equal("", line.Text));
    }

    [Fact]
    public void Add_PutsEachLineBelowTheLast()
    {
        lines.Add(new LogLine("Welcome!", 1));
        lines.Add(new LogLine("Have fun.", 2));

        Assert.Equal(new LogLine("Welcome!", 1), lines.Lines[0]);
        Assert.Equal(new LogLine("Have fun.", 2), lines.Lines[1]);
    }

    [Fact]
    public void Add_WrapsALongLineAtTheLastSpaceWithinFortyTwoCharacters()
    {
        lines.Add(new LogLine(LongSentence, 3));

        Assert.Equal(new LogLine("The quick brown fox jumps over the lazy", 3), lines.Lines[0]);
        Assert.Equal(new LogLine("dog and keeps running far away", 3), lines.Lines[1]);
    }

    [Fact]
    public void Add_KeepsAWordTooLongToWrapWholeAndRepeatsItsEndBelow()
    {
        lines.Add(new LogLine(new string('x', 50), 0));

        Assert.Equal(new string('x', 50), lines.Lines[0].Text);
        Assert.Equal(new string('x', 8), lines.Lines[1].Text);
    }

    [Fact]
    public void Add_KeepsAtMostFiftyNineCharactersOnALine()
    {
        lines.Add(new LogLine(new string('x', 70), 0));

        Assert.Equal(new string('x', 59), lines.Lines[0].Text);
        Assert.Equal(new string('x', 28), lines.Lines[1].Text);
    }

    [Fact]
    public void Add_IgnoresAnEmptyLineButKeepsASpace()
    {
        lines.Add(new LogLine("", 1));
        lines.Add(new LogLine(" ", 1));
        lines.Add(new LogLine("After", 1));

        Assert.Equal(" ", lines.Lines[0].Text);
        Assert.Equal("After", lines.Lines[1].Text);
    }

    [Fact]
    public void Add_ShowsOnlyTheFirstTwentySevenLines()
    {
        AddNumbered(30);

        Assert.Equal("line 26", lines.Lines[26].Text);
        Assert.DoesNotContain(lines.Lines, line => line.Text == "line 27");
    }

    [Fact]
    public void Add_StartsAgainAtTheTopAfterFiftyNineLines()
    {
        AddNumbered(60);

        Assert.Equal("line 59", lines.Lines[0].Text);
        Assert.Equal("line 1", lines.Lines[1].Text);
    }

    private void AddNumbered(int count)
    {
        for (var line = 0; line < count; line++)
        {
            lines.Add(new LogLine($"line {line}", 1));
        }
    }
}

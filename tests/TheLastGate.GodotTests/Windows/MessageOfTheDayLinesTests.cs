using GdUnit4;
using TheLastGate.State;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class MessageOfTheDayLinesTests
{
    private const string LongSentence = "The quick brown fox jumps over the lazy dog and keeps running far away";

    private MessageOfTheDayLines lines = new();

    [BeforeTest]
    public void StartEmpty() => lines = new MessageOfTheDayLines();

    [TestCase]
    public void Lines_StartEmpty()
    {
        AssertThat(lines.Lines.Count).IsEqual(MessageOfTheDayLines.VisibleLines);
        AssertThat(lines.Lines.Select(line => line.Text).Distinct()).ContainsExactly("");
    }

    [TestCase]
    public void Add_PutsEachLineBelowTheLast()
    {
        lines.Add(new LogLine("Welcome!", 1));
        lines.Add(new LogLine("Have fun.", 2));

        AssertThat(lines.Lines[0]).IsEqual(new LogLine("Welcome!", 1));
        AssertThat(lines.Lines[1]).IsEqual(new LogLine("Have fun.", 2));
    }

    [TestCase]
    public void Add_WrapsALongLineAtTheLastSpaceWithinFortyTwoCharacters()
    {
        lines.Add(new LogLine(LongSentence, 3));

        AssertThat(lines.Lines[0]).IsEqual(new LogLine("The quick brown fox jumps over the lazy", 3));
        AssertThat(lines.Lines[1]).IsEqual(new LogLine("dog and keeps running far away", 3));
    }

    [TestCase]
    public void Add_KeepsAWordTooLongToWrapWholeAndRepeatsItsEndBelow()
    {
        lines.Add(new LogLine(new string('x', 50), 0));

        AssertThat(lines.Lines[0].Text).IsEqual(new string('x', 50));
        AssertThat(lines.Lines[1].Text).IsEqual(new string('x', 8));
    }

    [TestCase]
    public void Add_KeepsAtMostFiftyNineCharactersOnALine()
    {
        lines.Add(new LogLine(new string('x', 70), 0));

        AssertThat(lines.Lines[0].Text).IsEqual(new string('x', 59));
        AssertThat(lines.Lines[1].Text).IsEqual(new string('x', 28));
    }

    [TestCase]
    public void Add_IgnoresAnEmptyLineButKeepsASpace()
    {
        lines.Add(new LogLine("", 1));
        lines.Add(new LogLine(" ", 1));
        lines.Add(new LogLine("After", 1));

        AssertThat(lines.Lines[0].Text).IsEqual(" ");
        AssertThat(lines.Lines[1].Text).IsEqual("After");
    }

    [TestCase]
    public void Add_ShowsOnlyTheFirstTwentySevenLines()
    {
        AddNumbered(30);

        AssertThat(lines.Lines[26].Text).IsEqual("line 26");
        AssertThat(lines.Lines.Select(line => line.Text)).NotContains("line 27");
    }

    [TestCase]
    public void Add_StartsAgainAtTheTopAfterFiftyNineLines()
    {
        AddNumbered(60);

        AssertThat(lines.Lines[0].Text).IsEqual("line 59");
        AssertThat(lines.Lines[1].Text).IsEqual("line 1");
    }

    private void AddNumbered(int count)
    {
        for (var line = 0; line < count; line++)
        {
            lines.Add(new LogLine($"line {line}", 1));
        }
    }
}

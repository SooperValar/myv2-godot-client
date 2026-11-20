using GdUnit4;
using TheLastGate.Chat;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Chat;

[TestSuite]
public class ChatHistoryTests
{
    [TestCase]
    public void Older_ShowsTheSentLinesNewestFirst()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Sent("two");

        AssertThat(history.Older("draft")).IsEqual("two");
        AssertThat(history.Older("two")).IsEqual("one");
    }

    [TestCase]
    public void Older_StopsAtTheOldestSentLine()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Older("draft");

        AssertThat(history.Older("one")).IsNull();
    }

    [TestCase]
    public void Older_ShowsNothingBeforeAnyLineIsSent()
    {
        AssertThat(new ChatHistory().Older("draft")).IsNull();
    }

    [TestCase]
    public void Newer_ReturnsToTheLineBeingTyped()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Older("draft");

        AssertThat(history.Newer("one")).IsEqual("draft");
    }

    [TestCase]
    public void Newer_ShowsNothingNewerThanTheLineBeingTyped()
    {
        var history = new ChatHistory();
        history.Sent("one");

        AssertThat(history.Newer("draft")).IsNull();
    }

    [TestCase]
    public void OlderAndNewer_KeepEditsMadeToSentLines()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Sent("two");
        history.Older("draft");

        history.Older("two edited");

        AssertThat(history.Newer("one")).IsEqual("two edited");
    }

    [TestCase]
    public void Sent_GoesBackToTheLineBeingTyped()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Older("");

        history.Sent("two");

        AssertThat(history.Older("")).IsEqual("two");
    }

    [TestCase]
    public void Sent_KeepsTheLastNineteenLines()
    {
        var history = new ChatHistory();
        for (var line = 0; line < 25; line++)
        {
            history.Sent($"line {line}");
        }

        var current = "";
        for (var step = 0; step < 19; step++)
        {
            current = history.Older(current)!;
        }

        AssertThat(current).IsEqual("line 6");
        AssertThat(history.Older(current)).IsNull();
    }

    [TestCase]
    public void Abandon_StartsAgainFromTheNewestSentLine()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Sent("two");
        history.Older("draft");
        history.Older("two");

        history.Abandon();

        AssertThat(history.Newer("")).IsNull();
        AssertThat(history.Older("")).IsEqual("two");
    }
}

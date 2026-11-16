using TheLastGate.Chat;

namespace TheLastGate.Tests;

public class ChatHistoryTests
{
    [Fact]
    public void Older_ShowsTheSentLinesNewestFirst()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Sent("two");

        Assert.Equal("two", history.Older("draft"));
        Assert.Equal("one", history.Older("two"));
    }

    [Fact]
    public void Older_StopsAtTheOldestSentLine()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Older("draft");

        Assert.Null(history.Older("one"));
    }

    [Fact]
    public void Older_ShowsNothingBeforeAnyLineIsSent()
    {
        Assert.Null(new ChatHistory().Older("draft"));
    }

    [Fact]
    public void Newer_ReturnsToTheLineBeingTyped()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Older("draft");

        Assert.Equal("draft", history.Newer("one"));
    }

    [Fact]
    public void Newer_ShowsNothingNewerThanTheLineBeingTyped()
    {
        var history = new ChatHistory();
        history.Sent("one");

        Assert.Null(history.Newer("draft"));
    }

    [Fact]
    public void OlderAndNewer_KeepEditsMadeToSentLines()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Sent("two");
        history.Older("draft");

        history.Older("two edited");

        Assert.Equal("two edited", history.Newer("one"));
    }

    [Fact]
    public void Sent_GoesBackToTheLineBeingTyped()
    {
        var history = new ChatHistory();
        history.Sent("one");
        history.Older("");

        history.Sent("two");

        Assert.Equal("two", history.Older(""));
    }

    [Fact]
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

        Assert.Equal("line 6", current);
        Assert.Null(history.Older(current));
    }
}

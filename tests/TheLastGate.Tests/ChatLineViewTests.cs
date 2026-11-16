using TheLastGate.Chat;

namespace TheLastGate.Tests;

public class ChatLineViewTests
{
    private const string Long = "This is a much longer line that will not fit in the forty-eight shown";

    [Fact]
    public void Show_ShowsAShortLineWholeWithTheCursorAtTheCaret()
    {
        Assert.Equal(("hello there", 2), new ChatLineView().Show("hello there", 2));
    }

    [Fact]
    public void Show_ScrollsSoTheCursorIsAtMostFortyFiveColumnsIn()
    {
        var shown = new ChatLineView().Show(Long, 60);

        Assert.Equal((Long.Substring(15, 48), 45), shown);
    }

    [Fact]
    public void Show_ScrollsBackSoTheCursorIsAtLeastFiveColumnsIn()
    {
        var view = new ChatLineView();
        view.Show(Long, 60);

        Assert.Equal((Long.Substring(13, 48), 5), view.Show(Long, 18));
    }

    [Fact]
    public void Show_KeepsTheScrollWhileTheCursorStaysInsideTheLeads()
    {
        var view = new ChatLineView();
        view.Show(Long, 60);

        Assert.Equal((Long.Substring(15, 48), 25), view.Show(Long, 40));
    }

    [Fact]
    public void Show_ScrollsBackToTheStartNearTheBeginning()
    {
        var view = new ChatLineView();
        view.Show(Long, 60);

        Assert.Equal((Long[..48], 3), view.Show(Long, 3));
    }

    [Fact]
    public void Show_ShowsTheEndOfALineWithTheCursorAfterIt()
    {
        var text = new string('a', 48);

        Assert.Equal((new string('a', 45), 45), new ChatLineView().Show(text, 48));
    }
}

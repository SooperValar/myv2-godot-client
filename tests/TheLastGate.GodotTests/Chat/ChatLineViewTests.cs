using GdUnit4;
using TheLastGate.Chat;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Chat;

[TestSuite]
public class ChatLineViewTests
{
    private const string Long = "This is a much longer line that will not fit in the forty-eight shown";

    [TestCase]
    public void Show_ShowsAShortLineWholeWithTheCursorAtTheCaret()
    {
        AssertThat(new ChatLineView().Show("hello there", 2)).IsEqual(("hello there", 2));
    }

    [TestCase]
    public void Show_ScrollsSoTheCursorIsAtMostFortyFiveColumnsIn()
    {
        var shown = new ChatLineView().Show(Long, 60);

        AssertThat(shown).IsEqual((Long.Substring(15, 48), 45));
    }

    [TestCase]
    public void Show_ScrollsBackSoTheCursorIsAtLeastFiveColumnsIn()
    {
        var view = new ChatLineView();
        view.Show(Long, 60);

        AssertThat(view.Show(Long, 18)).IsEqual((Long.Substring(13, 48), 5));
    }

    [TestCase]
    public void Show_KeepsTheScrollWhileTheCursorStaysInsideTheLeads()
    {
        var view = new ChatLineView();
        view.Show(Long, 60);

        AssertThat(view.Show(Long, 40)).IsEqual((Long.Substring(15, 48), 25));
    }

    [TestCase]
    public void Show_ScrollsBackToTheStartNearTheBeginning()
    {
        var view = new ChatLineView();
        view.Show(Long, 60);

        AssertThat(view.Show(Long, 3)).IsEqual((Long[..48], 3));
    }

    [TestCase]
    public void Show_ShowsTheEndOfALineWithTheCursorAfterIt()
    {
        var text = new string('a', 48);

        AssertThat(new ChatLineView().Show(text, 48)).IsEqual((new string('a', 45), 45));
    }
}

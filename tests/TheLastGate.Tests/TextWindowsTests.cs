using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class TextWindowsTests
{
    private static readonly WindowRequest MessageOfTheDay = new(WindowKind.MessageOfTheDay, 0);
    private static readonly WindowRequest Welcome = new(WindowKind.NewPlayerWelcome, 0);

    private readonly WindowPause pause = new();
    private readonly TextWindows windows;

    public TextWindowsTests() => windows = new TextWindows(pause);

    [Fact]
    public void Show_OpensTheMessageOfTheDay()
    {
        windows.Show(MessageOfTheDay);

        Assert.True(windows.MessageOfTheDayOpen);
        Assert.True(windows.AnyOpen);
    }

    [Fact]
    public void Show_OpensTheNewPlayerWelcome()
    {
        windows.Show(Welcome);

        Assert.True(windows.WelcomeOpen);
    }

    [Fact]
    public void Show_OpensATutorialAtItsFirstPage()
    {
        windows.Show(Tutorial(3));

        Assert.Equal(3, windows.Tutorial);
        Assert.Equal(1, windows.TutorialPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Show_IgnoresATutorialThatDoesNotExist(int tutorial)
    {
        windows.Show(Tutorial(tutorial));

        Assert.False(windows.AnyOpen);
    }

    [Fact]
    public void Show_LeavesBooksClosed()
    {
        windows.Show(new WindowRequest(WindowKind.Book, 5));

        Assert.False(windows.AnyOpen);
    }

    [Fact]
    public void AnyOpen_IsFalseAtFirst()
    {
        Assert.False(windows.AnyOpen);
    }

    [Fact]
    public void Close_ClosesEveryWindowWithoutTellingTheServerAboutAMessageOfTheDay()
    {
        windows.Show(MessageOfTheDay);
        windows.Show(Tutorial(2));

        var action = windows.Close();

        Assert.False(windows.AnyOpen);
        Assert.Equal([Seen(2)], action.Packets);
    }

    [Fact]
    public void Close_OnTheWelcomeSkipsTheTutorials()
    {
        windows.Show(Welcome);

        Assert.Equal([Seen(16)], windows.Close().Packets);
    }

    [Fact]
    public void Close_ReportsTheWelcomeThenTheTutorialWhenBothAreOpen()
    {
        windows.Show(Welcome);
        windows.Show(Tutorial(4));

        Assert.Equal([Seen(16), Seen(4)], windows.Close().Packets);
    }

    [Fact]
    public void Ok_ClosesOnlyTheMessageOfTheDay()
    {
        windows.Show(MessageOfTheDay);
        windows.Show(Tutorial(2));

        var action = windows.Ok();

        Assert.False(windows.MessageOfTheDayOpen);
        Assert.Equal(2, windows.Tutorial);
        Assert.Empty(action.Packets);
    }

    [Fact]
    public void Left_OnTheWelcomeOpensTheFirstTutorialWithAClick()
    {
        windows.Show(Welcome);

        var action = windows.Left();

        Assert.False(windows.WelcomeOpen);
        Assert.Equal(1, windows.Tutorial);
        Assert.Equal(1, windows.TutorialPage);
        Assert.Empty(action.Packets);
        Assert.True(action.PlaysClick);
    }

    [Fact]
    public void Right_OnTheWelcomeSkipsTheTutorialsAndCloses()
    {
        windows.Show(Welcome);

        var action = windows.Right();

        Assert.False(windows.WelcomeOpen);
        Assert.Equal([Seen(16)], action.Packets);
    }

    [Fact]
    public void Right_TurnsTutorialPagesClickingOnlyWhenItLandsOnTheMiddlePage()
    {
        windows.Show(Tutorial(1));

        Assert.True(windows.Right().PlaysClick);
        Assert.Equal(2, windows.TutorialPage);
        Assert.False(windows.Right().PlaysClick);
        Assert.Equal(3, windows.TutorialPage);
        Assert.False(windows.Right().PlaysClick);
        Assert.Equal(3, windows.TutorialPage);
    }

    [Fact]
    public void Left_TurnsTutorialPagesBackClickingOnlyWhenItLandsOnTheMiddlePage()
    {
        windows.Show(Tutorial(1));
        windows.Right();
        windows.Right();

        Assert.True(windows.Left().PlaysClick);
        Assert.Equal(2, windows.TutorialPage);
        Assert.False(windows.Left().PlaysClick);
        Assert.Equal(1, windows.TutorialPage);
        Assert.False(windows.Left().PlaysClick);
        Assert.Equal(1, windows.TutorialPage);
    }

    [Fact]
    public void Right_GoesToTheWelcomeBeforeATutorialOpenBehindIt()
    {
        windows.Show(Tutorial(2));
        windows.Show(Welcome);

        windows.Right();

        Assert.False(windows.WelcomeOpen);
        Assert.Equal(1, windows.TutorialPage);
    }

    [Fact]
    public void CloseByEscape_ClosesEverythingWithoutTellingTheServer()
    {
        windows.Show(Welcome);
        windows.Show(Tutorial(2));

        Assert.True(windows.CloseByEscape());
        Assert.False(windows.AnyOpen);
    }

    [Fact]
    public void CloseByEscape_SaysWhenNothingWasOpen()
    {
        Assert.False(windows.CloseByEscape());
    }

    [Fact]
    public void Close_StartsThePause()
    {
        windows.Show(MessageOfTheDay);

        windows.Close();

        Assert.True(pause.Active);
    }

    [Fact]
    public void Ok_StartsThePause()
    {
        windows.Show(MessageOfTheDay);

        windows.Ok();

        Assert.True(pause.Active);
    }

    [Fact]
    public void Right_SkippingTheWelcomeStartsThePause()
    {
        windows.Show(Welcome);

        windows.Right();

        Assert.True(pause.Active);
    }

    [Fact]
    public void TurningTutorialPages_StartsNoPause()
    {
        windows.Show(Tutorial(1));

        windows.Right();
        windows.Left();

        Assert.False(pause.Active);
    }

    [Fact]
    public void CloseByEscape_StartsNoPause()
    {
        windows.Show(MessageOfTheDay);

        windows.CloseByEscape();

        Assert.False(pause.Active);
    }

    [Fact]
    public void CloseAll_ClosesEveryWindowWithoutAPause()
    {
        windows.Show(MessageOfTheDay);
        windows.Show(Welcome);
        windows.Show(Tutorial(2));

        windows.CloseAll();

        Assert.False(windows.AnyOpen);
        Assert.False(pause.Active);
    }

    [Fact]
    public void TutorialPages_HoldTheTextAndPictureOfEveryPage()
    {
        Assert.Equal(TutorialPages.LinesPerPage, TutorialPages.Text(1, 1).Count);
        Assert.Equal("The diamond-shaped space in the middle of", TutorialPages.Text(1, 1)[0]);
        Assert.Equal("ours, and mayest thou pass The Last Gate!", TutorialPages.Text(6, 3)[11]);
        Assert.Equal(18010, TutorialPages.Picture(1, 1));
        Assert.Equal(18013, TutorialPages.Picture(2, 1));
        Assert.Equal(18027, TutorialPages.Picture(6, 3));
    }

    private static WindowRequest Tutorial(int number) => new(WindowKind.Tutorial, number);

    private static byte[] Seen(int value) => ClientPackets.OneValue(ClientCommand.MessageOfTheDay, value);
}

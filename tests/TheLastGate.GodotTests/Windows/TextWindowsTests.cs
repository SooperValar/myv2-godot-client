using GdUnit4;
using TheLastGate.Assets;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class TextWindowsTests
{
    private static readonly WindowRequest MessageOfTheDay = new(WindowKind.MessageOfTheDay, 0);
    private static readonly WindowRequest Welcome = new(WindowKind.NewPlayerWelcome, 0);

    private WindowPause pause = new();
    private TextWindows windows = null!;

    [BeforeTest]
    public void OpenNothing()
    {
        pause = new WindowPause();
        windows = new TextWindows(pause);
    }

    [TestCase]
    public void Show_OpensTheMessageOfTheDay()
    {
        windows.Show(MessageOfTheDay);

        AssertThat(windows.MessageOfTheDayOpen).IsTrue();
        AssertThat(windows.AnyOpen).IsTrue();
    }

    [TestCase]
    public void Show_OpensTheNewPlayerWelcome()
    {
        windows.Show(Welcome);

        AssertThat(windows.WelcomeOpen).IsTrue();
    }

    [TestCase]
    public void Show_OpensATutorialAtItsFirstPage()
    {
        windows.Show(Tutorial(3));

        AssertThat(windows.Tutorial).IsEqual(3);
        AssertThat(windows.TutorialPage).IsEqual(1);
    }

    [TestCase(0)]
    [TestCase(7)]
    public void Show_IgnoresATutorialThatDoesNotExist(int tutorial)
    {
        windows.Show(Tutorial(tutorial));

        AssertThat(windows.AnyOpen).IsFalse();
    }

    [TestCase]
    public void Show_LeavesBooksClosed()
    {
        windows.Show(new WindowRequest(WindowKind.Book, 5));

        AssertThat(windows.AnyOpen).IsFalse();
    }

    [TestCase]
    public void AnyOpen_IsFalseAtFirst()
    {
        AssertThat(windows.AnyOpen).IsFalse();
    }

    [TestCase]
    public void Close_ClosesEveryWindowWithoutTellingTheServerAboutAMessageOfTheDay()
    {
        windows.Show(MessageOfTheDay);
        windows.Show(Tutorial(2));

        var action = windows.Close();

        AssertThat(windows.AnyOpen).IsFalse();
        AssertThat(action.Packets).HasSize(1);
        AssertThat(action.Packets[0]).ContainsExactly(Seen(2));
    }

    [TestCase]
    public void Close_OnTheWelcomeSkipsTheTutorials()
    {
        windows.Show(Welcome);

        var packets = windows.Close().Packets;

        AssertThat(packets).HasSize(1);
        AssertThat(packets[0]).ContainsExactly(Seen(16));
    }

    [TestCase]
    public void Close_ReportsTheWelcomeThenTheTutorialWhenBothAreOpen()
    {
        windows.Show(Welcome);
        windows.Show(Tutorial(4));

        var packets = windows.Close().Packets;

        AssertThat(packets).HasSize(2);
        AssertThat(packets[0]).ContainsExactly(Seen(16));
        AssertThat(packets[1]).ContainsExactly(Seen(4));
    }

    [TestCase]
    public void Ok_ClosesOnlyTheMessageOfTheDay()
    {
        windows.Show(MessageOfTheDay);
        windows.Show(Tutorial(2));

        var action = windows.Ok();

        AssertThat(windows.MessageOfTheDayOpen).IsFalse();
        AssertThat(windows.Tutorial).IsEqual(2);
        AssertThat(action.Packets).IsEmpty();
    }

    [TestCase]
    public void Left_OnTheWelcomeOpensTheFirstTutorialWithAClick()
    {
        windows.Show(Welcome);

        var action = windows.Left();

        AssertThat(windows.WelcomeOpen).IsFalse();
        AssertThat(windows.Tutorial).IsEqual(1);
        AssertThat(windows.TutorialPage).IsEqual(1);
        AssertThat(action.Packets).IsEmpty();
        AssertThat(action.PlaysClick).IsTrue();
    }

    [TestCase]
    public void Right_OnTheWelcomeSkipsTheTutorialsAndCloses()
    {
        windows.Show(Welcome);

        var action = windows.Right();

        AssertThat(windows.WelcomeOpen).IsFalse();
        AssertThat(action.Packets).HasSize(1);
        AssertThat(action.Packets[0]).ContainsExactly(Seen(16));
    }

    [TestCase]
    public void Right_TurnsTutorialPagesClickingOnlyWhenItLandsOnTheMiddlePage()
    {
        windows.Show(Tutorial(1));

        AssertThat(windows.Right().PlaysClick).IsTrue();
        AssertThat(windows.TutorialPage).IsEqual(2);
        AssertThat(windows.Right().PlaysClick).IsFalse();
        AssertThat(windows.TutorialPage).IsEqual(3);
        AssertThat(windows.Right().PlaysClick).IsFalse();
        AssertThat(windows.TutorialPage).IsEqual(3);
    }

    [TestCase]
    public void Left_TurnsTutorialPagesBackClickingOnlyWhenItLandsOnTheMiddlePage()
    {
        windows.Show(Tutorial(1));
        windows.Right();
        windows.Right();

        AssertThat(windows.Left().PlaysClick).IsTrue();
        AssertThat(windows.TutorialPage).IsEqual(2);
        AssertThat(windows.Left().PlaysClick).IsFalse();
        AssertThat(windows.TutorialPage).IsEqual(1);
        AssertThat(windows.Left().PlaysClick).IsFalse();
        AssertThat(windows.TutorialPage).IsEqual(1);
    }

    [TestCase]
    public void Right_GoesToTheWelcomeBeforeATutorialOpenBehindIt()
    {
        windows.Show(Tutorial(2));
        windows.Show(Welcome);

        windows.Right();

        AssertThat(windows.WelcomeOpen).IsFalse();
        AssertThat(windows.TutorialPage).IsEqual(1);
    }

    [TestCase]
    public void CloseByEscape_ClosesEverythingWithoutTellingTheServer()
    {
        windows.Show(Welcome);
        windows.Show(Tutorial(2));

        AssertThat(windows.CloseByEscape()).IsTrue();
        AssertThat(windows.AnyOpen).IsFalse();
    }

    [TestCase]
    public void CloseByEscape_SaysWhenNothingWasOpen()
    {
        AssertThat(windows.CloseByEscape()).IsFalse();
    }

    [TestCase]
    public void Close_StartsThePause()
    {
        windows.Show(MessageOfTheDay);

        windows.Close();

        AssertThat(pause.Active).IsTrue();
    }

    [TestCase]
    public void Ok_StartsThePause()
    {
        windows.Show(MessageOfTheDay);

        windows.Ok();

        AssertThat(pause.Active).IsTrue();
    }

    [TestCase]
    public void Right_SkippingTheWelcomeStartsThePause()
    {
        windows.Show(Welcome);

        windows.Right();

        AssertThat(pause.Active).IsTrue();
    }

    [TestCase]
    public void TurningTutorialPages_StartsNoPause()
    {
        windows.Show(Tutorial(1));

        windows.Right();
        windows.Left();

        AssertThat(pause.Active).IsFalse();
    }

    [TestCase]
    public void CloseByEscape_StartsNoPause()
    {
        windows.Show(MessageOfTheDay);

        windows.CloseByEscape();

        AssertThat(pause.Active).IsFalse();
    }

    [TestCase]
    public void CloseAll_ClosesEveryWindowWithoutAPause()
    {
        windows.Show(MessageOfTheDay);
        windows.Show(Welcome);
        windows.Show(Tutorial(2));

        windows.CloseAll();

        AssertThat(windows.AnyOpen).IsFalse();
        AssertThat(pause.Active).IsFalse();
    }

    [TestCase]
    public void TutorialPages_HoldTheTextAndPictureOfEveryPage()
    {
        AssertThat(TutorialPages.Text(1, 1).Count).IsEqual(TutorialPages.LinesPerPage);
        AssertThat(TutorialPages.Text(1, 1)[0]).IsEqual("The diamond-shaped space in the middle of");
        AssertThat(TutorialPages.Text(6, 3)[11]).IsEqual("ours, and mayest thou pass The Last Gate!");
        AssertThat(TutorialPages.Picture(1, 1)).IsEqual(SpriteNumbers.ClientArtBase + 18010);
        AssertThat(TutorialPages.Picture(2, 1)).IsEqual(SpriteNumbers.ClientArtBase + 18013);
        AssertThat(TutorialPages.Picture(6, 3)).IsEqual(SpriteNumbers.ClientArtBase + 18027);
    }

    private static WindowRequest Tutorial(int number) => new(WindowKind.Tutorial, number);

    private static byte[] Seen(int value) => ClientPackets.OneValue(ClientCommand.MessageOfTheDay, value);
}

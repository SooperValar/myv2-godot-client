using GdUnit4;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class WorldMapWindowTests
{
    private WorldMapWindow window = new();

    [BeforeTest]
    public void CloseTheMap() => window = new WorldMapWindow();

    [TestCase]
    public void OpenAtPlayer_OpensItCentredOnThePlayer()
    {
        window.OpenAtPlayer();

        AssertThat(window.IsOpen).IsTrue();
        AssertThat(window.CentreOnPlayer).IsTrue();
    }

    [TestCase]
    public void Toggle_OpensAtThePlayerOrCloses()
    {
        window.Toggle();
        AssertThat(window.IsOpen).IsTrue();
        AssertThat(window.CentreOnPlayer).IsTrue();

        window.Toggle();
        AssertThat(window.IsOpen).IsFalse();
    }

    [TestCase]
    public void Close_SaysWhetherItWasOpen()
    {
        window.OpenAtPlayer();

        AssertThat(window.Close()).IsTrue();
        AssertThat(window.Close()).IsFalse();
    }

    [TestCase]
    public void ANewWindow_StartsWithTheSelectToolAndNotFollowing()
    {
        AssertThat(window.Tool).IsEqual(WorldMapTool.Select);
        AssertThat(window.Follow).IsFalse();
    }

    [TestCase(WorldMapTool.Select, false, true)]
    [TestCase(WorldMapTool.Pan, false, true)]
    [TestCase(WorldMapTool.Pin, false, false)]
    [TestCase(WorldMapTool.Line, true, true)]
    [TestCase(WorldMapTool.Text, true, true)]
    public void DrawingTools_NeedACategoryToDrawInto(WorldMapTool tool, bool hasCategory, bool available)
    {
        AssertThat(WorldMapWindow.IsAvailable(tool, hasCategory)).IsEqual(available);
    }

    [TestCase]
    public void Panning_StopsFollowingThePlayer()
    {
        window.Follow = true;

        window.Panned();

        AssertThat(window.Follow).IsFalse();
    }

    [TestCase]
    public void AStatus_ShowsForThreeSeconds()
    {
        window.ShowStatus("Category is full.", 10);

        AssertThat(window.StatusAt(12.9)).IsEqual("Category is full.");
        AssertThat(window.StatusAt(13)).IsEqual("");
    }

    [TestCase]
    public void TheSetAsideNotice_ShowsForTwelveSecondsNamingTheFile()
    {
        window.ShowSetAsideNotice("TLGAnnotations_tlg.abyss.codes_5555.dat", 10);

        AssertThat(window.StatusAt(21.9))
            .IsEqual(
                "Your saved annotations could not be read and were set aside as TLGAnnotations_tlg.abyss.codes_5555.dat.bad.");
        AssertThat(window.StatusAt(22)).IsEqual("");
    }

    [TestCase]
    public void Help_StartsOnTheFirstPageAndStopsAtTheEnds()
    {
        window.OpenHelp();
        window.PreviousHelpPage();
        AssertThat(window.HelpPage).IsEqual(0);

        for (var page = 0; page < 10; page++)
        {
            window.NextHelpPage();
        }

        AssertThat(window.HelpPage).IsEqual(WorldMapHelp.Pages.Count - 1);
        AssertThat(window.HelpPageText).IsEqual("8 / 8");

        window.OpenHelp();
        AssertThat(window.HelpPage).IsEqual(0);
        AssertThat(window.HelpPageText).IsEqual("1 / 8");
    }

    [TestCase]
    public void Help_HasTheCurrentClientsEightPages()
    {
        AssertThat(WorldMapHelp.Pages.Select(page => page.Title))
            .ContainsExactly(
                "Getting around",
                "Categories",
                "Category menu",
                "Pins and labels",
                "Drawing paths",
                "Changing annotations",
                "Edit mode",
                "Sharing and the minimap");
        AssertThat(WorldMapHelp.Pages[0].Lines[1])
            .IsEqual("Scroll the mouse wheel to zoom in and out around the cursor.");
        AssertThat(WorldMapHelp.Pages[7].Lines.Count).IsEqual(7);
    }
}

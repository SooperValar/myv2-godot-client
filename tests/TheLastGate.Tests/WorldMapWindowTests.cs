using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class WorldMapWindowTests
{
    private readonly WorldMapWindow window = new();

    [Fact]
    public void OpenAtPlayer_OpensItCentredOnThePlayer()
    {
        window.OpenAtPlayer();

        Assert.True(window.IsOpen);
        Assert.True(window.CentreOnPlayer);
    }

    [Fact]
    public void Toggle_OpensAtThePlayerOrCloses()
    {
        window.Toggle();
        Assert.True(window.IsOpen);
        Assert.True(window.CentreOnPlayer);

        window.Toggle();
        Assert.False(window.IsOpen);
    }

    [Fact]
    public void Close_SaysWhetherItWasOpen()
    {
        window.OpenAtPlayer();

        Assert.True(window.Close());
        Assert.False(window.Close());
    }

    [Fact]
    public void Escape_ClosesTheWorldMapWithTheOtherWindows()
    {
        var state = new GameState();
        state.WorldMapWindow.OpenAtPlayer();

        Assert.True(state.CloseWindowsByEscape());

        Assert.False(state.WorldMapWindow.IsOpen);
    }

    [Fact]
    public void ANewWindow_StartsWithTheSelectToolAndNotFollowing()
    {
        Assert.Equal(WorldMapTool.Select, window.Tool);
        Assert.False(window.Follow);
    }

    [Theory]
    [InlineData(WorldMapTool.Select, false, true)]
    [InlineData(WorldMapTool.Pan, false, true)]
    [InlineData(WorldMapTool.Pin, false, false)]
    [InlineData(WorldMapTool.Line, true, true)]
    [InlineData(WorldMapTool.Text, true, true)]
    public void DrawingTools_NeedACategoryToDrawInto(WorldMapTool tool, bool hasCategory, bool available)
    {
        Assert.Equal(available, WorldMapWindow.IsAvailable(tool, hasCategory));
    }

    [Fact]
    public void Panning_StopsFollowingThePlayer()
    {
        window.Follow = true;

        window.Panned();

        Assert.False(window.Follow);
    }

    [Fact]
    public void AStatus_ShowsForThreeSeconds()
    {
        window.ShowStatus("Category is full.", 10);

        Assert.Equal("Category is full.", window.StatusAt(12.9));
        Assert.Equal("", window.StatusAt(13));
    }

    [Fact]
    public void TheSetAsideNotice_ShowsForTwelveSecondsNamingTheFile()
    {
        window.ShowSetAsideNotice("TLGAnnotations_tlg.abyss.codes_5555.dat", 10);

        Assert.Equal(
            "Your saved annotations could not be read and were set aside as TLGAnnotations_tlg.abyss.codes_5555.dat.bad.",
            window.StatusAt(21.9));
        Assert.Equal("", window.StatusAt(22));
    }

    [Fact]
    public void Help_StartsOnTheFirstPageAndStopsAtTheEnds()
    {
        window.OpenHelp();
        window.PreviousHelpPage();
        Assert.Equal(0, window.HelpPage);

        for (var page = 0; page < 10; page++)
        {
            window.NextHelpPage();
        }

        Assert.Equal(WorldMapHelp.Pages.Count - 1, window.HelpPage);
        Assert.Equal("8 / 8", window.HelpPageText);

        window.OpenHelp();
        Assert.Equal(0, window.HelpPage);
        Assert.Equal("1 / 8", window.HelpPageText);
    }

    [Fact]
    public void Help_HasTheCurrentClientsEightPages()
    {
        Assert.Equal(
            [
                "Getting around", "Categories", "Category menu", "Pins and labels", "Drawing paths",
                "Changing annotations", "Edit mode", "Sharing and the minimap",
            ],
            WorldMapHelp.Pages.Select(page => page.Title));
        Assert.Equal(
            "Scroll the mouse wheel to zoom in and out around the cursor.",
            WorldMapHelp.Pages[0].Lines[1]);
        Assert.Equal(7, WorldMapHelp.Pages[7].Lines.Count);
    }
}

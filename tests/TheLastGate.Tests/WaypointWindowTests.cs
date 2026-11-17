using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class WaypointWindowTests
{
    private const int LynboreTavern = 0;
    private const int LizardTempleSouth = 20;
    private const int NothingFound = 0;

    private readonly GameState state = new();
    private readonly WaypointWindow window = new(new WindowPause());

    [Fact]
    public void Waypoints_ListsEveryNumberOnceInTheWindowsOrder()
    {
        Assert.Equal(Waypoints.Count, Waypoints.All.Count);
        Assert.Equal(Enumerable.Range(0, Waypoints.Count), Waypoints.All.Select(waypoint => waypoint.Number).Order());
        Assert.Equal(new Waypoint(0, "Lynbore, Tavern", "\"Humble beginnings.\""), Waypoints.All[0]);
        Assert.Equal(new Waypoint(20, "Lizard Temple, South", "\"Where reptiles implore.\""), Waypoints.All[7]);
        Assert.Equal(new Waypoint(26, "The Burning Plains", "\"The archon waits.\""), Waypoints.All[^1]);
    }

    [Theory]
    [InlineData(0b101, 0, true)]
    [InlineData(0b101, 1, false)]
    [InlineData(0b101, 2, true)]
    [InlineData(1 << 26, 26, true)]
    public void Found_ReadsTheWaypointsBit(int foundMask, int number, bool found)
    {
        Assert.Equal(found, Waypoints.Found(foundMask, number));
    }

    [Fact]
    public void OpenAndClose_ShowAndHideTheWindow()
    {
        window.Open();
        Assert.True(window.IsOpen);

        Assert.True(window.Close());
        Assert.False(window.IsOpen);
        Assert.False(window.Close());
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(-1, 0)]
    [InlineData(25, WaypointWindow.MaxScroll)]
    public void ScrollBy_StaysWithinTheList(int rows, int scroll)
    {
        window.ScrollBy(rows);

        Assert.Equal(scroll, window.Scroll);
    }

    [Theory]
    [InlineData(MapKeys.None, 2)]
    [InlineData(MapKeys.Shift, 8)]
    [InlineData(MapKeys.Ctrl, 8)]
    [InlineData(MapKeys.Alt, 8)]
    public void ArrowStep_ScrollsFurtherWithAModifierHeld(MapKeys keys, int rows)
    {
        Assert.Equal(rows, WaypointWindow.ArrowStep(keys));
    }

    [Theory]
    [InlineData(0, 36)]
    [InlineData(10, 128)]
    [InlineData(WaypointWindow.MaxScroll, 212)]
    public void BarTop_SlidesTheBarDownTheScrollbar(int scroll, int top)
    {
        Assert.Equal(top, WaypointWindow.BarTop(scroll));
    }

    [Fact]
    public void At_CountsRowsFromTheScrollPosition()
    {
        window.ScrollBy(7);

        Assert.Equal(LizardTempleSouth, window.At(0).Number);
    }

    [Fact]
    public void Scroll_IsKeptWhenTheWindowReopens()
    {
        window.Open();
        window.ScrollBy(5);
        window.Close();

        window.Open();

        Assert.Equal(5, window.Scroll);
    }

    [Fact]
    public void Choose_TravelsToAFoundWaypointThenClosesWithThePause()
    {
        window.Open();
        window.ScrollBy(7);

        var packet = window.Choose(0, MapButton.Left, 1 << LizardTempleSouth);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, LizardTempleSouth), packet);
        Assert.False(window.IsOpen);
        Assert.True(window.Pause.Active);
    }

    [Fact]
    public void Choose_AsksAboutAnUnknownWaypointAndStaysOpen()
    {
        window.Open();

        var packet = window.Choose(0, MapButton.Left, NothingFound);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, LynboreTavern), packet);
        Assert.True(window.IsOpen);
        Assert.False(window.Pause.Active);
    }

    [Fact]
    public void Choose_DescribesAWaypointOnARightClick()
    {
        window.Open();
        window.ScrollBy(7);

        var packet = window.Choose(0, MapButton.Right, 1 << LizardTempleSouth);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, LizardTempleSouth + 32), packet);
        Assert.True(window.IsOpen);
    }

    [Fact]
    public void Choose_SendsNothingForOtherButtons()
    {
        window.Open();

        Assert.Null(window.Choose(0, MapButton.None, NothingFound));
    }

    [Fact]
    public void TheServer_OpensTheWindow()
    {
        Process(TestCommand.Of(ServerCommand.Waypoints));

        Assert.True(state.WaypointWindow.IsOpen);
    }

    [Fact]
    public void OpeningAShop_ClosesTheWindow()
    {
        state.WaypointWindow.Open();

        OpenShop();

        Assert.NotNull(state.OpenShop);
        Assert.False(state.WaypointWindow.IsOpen);
    }

    [Fact]
    public void Escape_ClosesTheWindowWithoutAPause()
    {
        state.WaypointWindow.Open();

        Assert.True(state.CloseWindowsByEscape());

        Assert.False(state.WaypointWindow.IsOpen);
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void ThePause_KeepsTheWindowShut()
    {
        state.Pause.Start();

        Process(TestCommand.Of(ServerCommand.Waypoints));

        Assert.False(state.WaypointWindow.IsOpen);
    }

    private void OpenShop() => Process(ShopCommands());

    private static TestCommand[] ShopCommands() =>
    [
        TestCommand.Of(ServerCommand.Look1),
        TestCommand.Of(ServerCommand.Look2),
        TestCommand.Of(ServerCommand.Look3).UInt16(0).UInt16(0).UInt16(0).UInt16(1201).UInt16(7),
        TestCommand.Of(ServerCommand.Look4).UInt16(0).UInt16(0).Byte((byte)LookFlags.Shop).UInt32(250),
        TestCommand.Of(ServerCommand.Look5).Text("Merchant", 15),
        .. Enumerable.Range(0, CharacterLook.ShopSlots / 2)
            .Select(pair => TestCommand.Of(ServerCommand.Look6)
                .Byte((byte)(pair * 2))
                .UInt16(1000)
                .UInt32(10)
                .UInt16(1001)
                .UInt32(20)
                .Bytes(0, 0)),
    ];

    private void Process(params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}

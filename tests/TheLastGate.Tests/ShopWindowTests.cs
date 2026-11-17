using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class ShopWindowTests
{
    private const int ShopNumber = 1201;
    private readonly TickProcessor processor;

    private readonly GameState state = new();

    public ShopWindowTests() => processor = new TickProcessor(state);

    [Fact]
    public void CloseShop_ClosesTheShopAndStartsThePause()
    {
        OpenShop();
        var closed = 0;
        state.ShopClosed += () => closed++;

        Assert.True(state.CloseShop());

        Assert.Null(state.OpenShop);
        Assert.Equal(1, closed);
        Assert.True(state.Pause.Active);
    }

    [Fact]
    public void CloseShop_DoesNothingWithoutAShop()
    {
        Assert.False(state.CloseShop());
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void CloseWindowsByEscape_ClosesTheShopWithAPauseAndTheTextWindowsWithout()
    {
        OpenShop();
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        Assert.True(state.CloseWindowsByEscape());

        Assert.Null(state.OpenShop);
        Assert.False(state.TextWindows.AnyOpen);
        Assert.True(state.Pause.Active);
    }

    [Fact]
    public void CloseWindowsByEscape_StartsNoPauseForTextWindowsAlone()
    {
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        Assert.True(state.CloseWindowsByEscape());
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void CloseWindowsByEscape_SaysWhenNothingWasOpen()
    {
        Assert.False(state.CloseWindowsByEscape());
    }

    [Fact]
    public void OpeningAShop_ClosesTheTextWindows()
    {
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        OpenShop();

        Assert.False(state.TextWindows.AnyOpen);
    }

    [Fact]
    public void TheServerClosingTheShop_StartsThePause()
    {
        OpenShop();

        Process(TestCommand.Of(ServerCommand.CloseShop));

        Assert.Null(state.OpenShop);
        Assert.True(state.Pause.Active);
    }

    [Theory]
    [InlineData(23, false)]
    [InlineData(24, true)]
    public void ThePause_KeepsShopsAndTextWindowsShutUntilItEnds(int ticksAfterClosing, bool staysOpen)
    {
        OpenShop();
        state.CloseShop();
        for (var tick = 0; tick < ticksAfterClosing; tick++)
        {
            Process();
        }

        Process([.. ShopCommands(), TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(0)]);

        Assert.Equal(staysOpen, state.OpenShop != null);
        Assert.Equal(staysOpen, state.TextWindows.MessageOfTheDayOpen);
    }

    [Fact]
    public void ShopRefresh_AsksForTheOpenShopEveryNinthTick()
    {
        var refresh = new ShopRefresh();
        var sent = new List<byte[]>();
        refresh.Send = sent.Add;

        for (var tick = 0; tick < 8; tick++)
        {
            refresh.EndOfTick(ShopKind.Merchant, ShopNumber);
        }

        Assert.Empty(sent);
        refresh.EndOfTick(ShopKind.Merchant, ShopNumber);
        Assert.Equal([ClientPackets.OneValue(ClientCommand.Look, ShopNumber)], sent);
    }

    [Fact]
    public void ShopRefresh_AsksNothingWithoutAShop()
    {
        var refresh = new ShopRefresh();
        var sent = new List<byte[]>();
        refresh.Send = sent.Add;

        for (var tick = 0; tick < 30; tick++)
        {
            refresh.EndOfTick(null, ShopNumber);
        }

        Assert.Empty(sent);
    }

    [Fact]
    public void EachTick_RefreshesTheOpenShop()
    {
        var sent = new List<byte[]>();
        state.ShopRefresh.Send = sent.Add;
        OpenShop();

        for (var tick = 0; tick < 9; tick++)
        {
            Process();
        }

        Assert.Contains(ClientPackets.OneValue(ClientCommand.Look, ShopNumber), sent);
    }

    private void OpenShop() => Process(ShopCommands());

    private static TestCommand[] ShopCommands() =>
    [
        TestCommand.Of(ServerCommand.Look1),
        TestCommand.Of(ServerCommand.Look2),
        TestCommand.Of(ServerCommand.Look3).UInt16(0).UInt16(0).UInt16(0).UInt16(ShopNumber).UInt16(7),
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

    private void Process(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));
}

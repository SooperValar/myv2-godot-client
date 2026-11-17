using System;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.Windows;

namespace TheLastGate.State;

public sealed class GameState
{
    public const int ServerTickCycle = 200;

    public GameState()
    {
        Tooltips = new ItemTooltips(Chat);
        MessageOfTheDay.LineAdded += MessageOfTheDayLines.Add;
        TextWindows = new TextWindows(Pause);
    }

    public PlayerState Player { get; } = new();
    public WorldMap Map { get; } = new();
    public TextLog Chat { get; } = new();
    public TextLog MessageOfTheDay { get; } = new();
    public Terminology Terminology { get; } = new();
    public CharacterNames Names { get; } = new();
    public ItemTooltips Tooltips { get; }
    public MessageOfTheDayLines MessageOfTheDayLines { get; } = new();
    public WindowPause Pause { get; } = new();
    public TextWindows TextWindows { get; }
    public ShopRefresh ShopRefresh { get; } = new();
    public CharacterLook? Look { get; internal set; }
    public CharacterLook Shop { get; internal set; } = new();
    public ShopKind? OpenShop { get; internal set; }
    public int ServerTick { get; internal set; }
    public int ClientTick { get; internal set; }
    public int ServerLoad { get; internal set; }

    internal CharacterLook PendingLook { get; } = new();

    public event Action<CharacterLook>? LookShown;
    public event Action<ShopKind>? ShopOpened;
    public event Action? ShopClosed;
    public event Action<SoundRequest>? SoundRequested;
    public event Action<ServerExit>? ExitRequested;
    public event Action<UniqueId>? UniqueIdReceived;
    public event Action? WaypointsRequested;
    public event Action<WindowRequest>? WindowRequested;
    public event Action<MinimapBox>? MinimapBoxCleared;
    public event Action<string>? UnusualDataSeen;

    public bool CloseShop()
    {
        if (OpenShop == null)
        {
            return false;
        }

        OpenShop = null;
        Pause.Start();
        ReportShopClosed();
        return true;
    }

    // Escape closes a shop with the pause, as the close button does, but tells nobody about the text windows.
    public bool CloseWindowsByEscape()
    {
        var closedText = TextWindows.CloseByEscape();
        var closedShop = CloseShop();
        return closedText || closedShop;
    }

    internal void ShutWindows()
    {
        TextWindows.CloseAll();
        if (OpenShop != null)
        {
            OpenShop = null;
            ReportShopClosed();
        }
    }

    internal void ReportLookShown(CharacterLook look) => LookShown?.Invoke(look);

    internal void ReportShopOpened(ShopKind kind) => ShopOpened?.Invoke(kind);

    internal void ReportShopClosed() => ShopClosed?.Invoke();

    internal void ReportSoundRequested(SoundRequest sound) => SoundRequested?.Invoke(sound);

    internal void ReportExitRequested(ServerExit exit) => ExitRequested?.Invoke(exit);

    internal void ReportUniqueIdReceived(UniqueId uniqueId) => UniqueIdReceived?.Invoke(uniqueId);

    internal void ReportWaypointsRequested() => WaypointsRequested?.Invoke();

    internal void ReportWindowRequested(WindowRequest window) => WindowRequested?.Invoke(window);

    internal void ReportMinimapBoxCleared(MinimapBox box) => MinimapBoxCleared?.Invoke(box);

    internal void ReportUnusualData(string description) => UnusualDataSeen?.Invoke(description);
}

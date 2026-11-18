using System;

namespace TheLastGate.Windows;

public enum WorldMapTool
{
    Select,
    Pan,
    Pin,
    Line,
    Text,
}

// Opened by its hotkey or the minimap, always centred on the player; Escape closes it with the other windows.
public sealed class WorldMapWindow
{
    private const double StatusSeconds = 3;
    private const double NoticeSeconds = 12;

    private string status = "";
    private double statusUntil;

    public bool IsOpen { get; private set; }

    public bool CentreOnPlayer { get; set; }

    public bool Follow { get; set; }

    public WorldMapTool Tool { get; set; }

    public WorldMapEditor Editor { get; } = new();

    public int HelpPage { get; private set; }

    public string HelpPageText => $"{HelpPage + 1} / {WorldMapHelp.Pages.Count}";

    // Pins, lines and text go into the current category, so they wait until there is one.
    public static bool IsAvailable(WorldMapTool tool, bool hasCategory) =>
        tool is WorldMapTool.Select or WorldMapTool.Pan || hasCategory;

    public void OpenAtPlayer()
    {
        IsOpen = true;
        CentreOnPlayer = true;
    }

    public void Toggle()
    {
        if (!Close())
        {
            OpenAtPlayer();
        }
    }

    public bool Close()
    {
        var wasOpen = IsOpen;
        IsOpen = false;
        return wasOpen;
    }

    public bool CloseByEscape() => throw new NotImplementedException();

    public void Panned() => Follow = false;

    public void ShowStatus(string text, double now) => Show(text, now, StatusSeconds);

    public void ShowSetAsideNotice(string fileName, double now) =>
        Show($"Your saved annotations could not be read and were set aside as {fileName}.bad.", now, NoticeSeconds);

    public string StatusAt(double now) => now < statusUntil ? status : "";

    public void OpenHelp() => HelpPage = 0;

    public void NextHelpPage() => HelpPage = Math.Min(HelpPage + 1, WorldMapHelp.Pages.Count - 1);

    public void PreviousHelpPage() => HelpPage = Math.Max(HelpPage - 1, 0);

    private void Show(string text, double now, double seconds)
    {
        status = text;
        statusUntil = now + seconds;
    }
}

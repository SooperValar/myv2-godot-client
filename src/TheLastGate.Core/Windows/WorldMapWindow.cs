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

public sealed class WorldMapWindow
{
    public bool IsOpen => throw new NotImplementedException();

    public bool CentreOnPlayer { get; set; }

    public bool Follow { get; set; }

    public WorldMapTool Tool { get; set; }

    public int HelpPage => throw new NotImplementedException();

    public string HelpPageText => throw new NotImplementedException();

    public static bool IsAvailable(WorldMapTool tool, bool hasCategory) => throw new NotImplementedException();

    public void OpenAtPlayer() => throw new NotImplementedException();

    public void Toggle() => throw new NotImplementedException();

    public bool Close() => throw new NotImplementedException();

    public void Panned() => throw new NotImplementedException();

    public void ShowStatus(string text, double now) => throw new NotImplementedException();

    public void ShowSetAsideNotice(string fileName, double now) => throw new NotImplementedException();

    public string StatusAt(double now) => throw new NotImplementedException();

    public void OpenHelp() => throw new NotImplementedException();

    public void NextHelpPage() => throw new NotImplementedException();

    public void PreviousHelpPage() => throw new NotImplementedException();
}

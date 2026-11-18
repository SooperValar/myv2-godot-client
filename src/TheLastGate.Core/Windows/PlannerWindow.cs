using System;
using TheLastGate.Net;
using TheLastGate.Planner;

namespace TheLastGate.Windows;

// The skill planner, opened by its button under the skill list or its hotkey while the option is on, and its Advanced
// window. Closing the planner closes both.
public sealed class PlannerWindow
{
    private bool askedForMeta;

    public bool IsOpen { get; private set; }

    public bool AdvancedOpen { get; set; }

    public PlannerFocus? Focus { get; private set; }

    public void ToggleFocus(PlannerFocus focus) => throw new NotImplementedException();

    public void ClearFocus() => throw new NotImplementedException();

    // The planner shows meta stats by name, which the server only sends when asked, so the first open asks.
    public bool Toggle()
    {
        if (IsOpen)
        {
            Close();
            return false;
        }

        IsOpen = true;
        var askNow = !askedForMeta;
        askedForMeta = true;
        return askNow;
    }

    public bool Close()
    {
        var wasOpen = IsOpen || AdvancedOpen;
        IsOpen = false;
        AdvancedOpen = false;
        return wasOpen;
    }

    public static string Header(int pointsLeft, int planned) => $"Exp left {pointsLeft}    Planned {planned}";

    public static byte[] MetaRequest() => ClientPackets.ShortAndInt(ClientCommand.Meta, 0, 0);
}

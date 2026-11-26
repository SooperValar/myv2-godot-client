using System;
using System.Collections.Generic;
using TheLastGate.Net;

namespace TheLastGate.Hud;

public sealed class ExitSequence
{

    public static readonly TimeSpan LeaveAfter = TimeSpan.FromSeconds(5);

    private bool exitRequested;
    private bool serverEnded;
    private TimeSpan sinceRequest;
    private bool warned;

    public static IReadOnlyList<string> WarningLines { get; } =
    [
        " ",
        "(F12) Leaving the game without entering a tavern will make you lose money and possibly life. Click again if you still want to leave the hard way.",
        "A tavern is located west of the Temple of Skua (the starting point).",
    ];

    public static IReadOnlyList<string> RequestLines { get; } =
        [" ", "(F12) Exit request acknowledged. Please wait a moment..."];

    public ExitStep Click()
    {
        if (serverEnded || (exitRequested && sinceRequest >= LeaveAfter))
        {
            return ExitStep.Leave;
        }

        if (!warned)
        {
            warned = true;
            return ExitStep.Warn;
        }

        if (!exitRequested)
        {
            exitRequested = true;
            return ExitStep.RequestExit;
        }

        return ExitStep.Wait;
    }

    public bool ServerEnded(int reasonCode)
    {
        serverEnded = true;
        return exitRequested || reasonCode is ExitReasons.EnteredTavern or ExitReasons.UserRequest;
    }

    public void Advance(TimeSpan elapsed)
    {
        if (exitRequested)
        {
            sinceRequest += elapsed;
        }
    }
}

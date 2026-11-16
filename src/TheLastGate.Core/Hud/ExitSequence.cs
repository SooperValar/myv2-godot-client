using System;
using System.Collections.Generic;
using TheLastGate.Net;

namespace TheLastGate.Hud;

public sealed class ExitSequence
{
    public const string TavernCommand = "/tavern";

    private const int EnteredTavernCode = 10;
    private const int UserRequestCode = 12;

    public static readonly TimeSpan LeaveAfter = TimeSpan.FromSeconds(5);

    private bool exitRequested;
    private bool serverEnded;
    private TimeSpan sinceRequest;
    private bool warned;

    public static IReadOnlyList<string> WarningLines { get; } =
    [
        " ",
        "(F12) Leaving the game without entering a tavern will cause you to take damage, and possibly lose your life. Click again if you need to leave immediately.",
        "A tavern will be located near where you entered the game.",
    ];

    public static IReadOnlyList<string> RequestLines { get; } =
        [" ", "(F12) Exit request acknowledged. Please wait a moment..."];

    public static byte[] ExitPacket => ClientPackets.OneValue(ClientCommand.Exit, 0);

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
        return exitRequested || reasonCode is EnteredTavernCode or UserRequestCode;
    }

    public void Advance(TimeSpan elapsed)
    {
        if (exitRequested)
        {
            sinceRequest += elapsed;
        }
    }
}

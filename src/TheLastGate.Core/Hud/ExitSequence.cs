using System;
using System.Collections.Generic;

namespace TheLastGate.Hud;

public sealed class ExitSequence
{
    public const string TavernCommand = "/tavern";

    public static readonly TimeSpan LeaveAfter = TimeSpan.FromSeconds(5);

    public static IReadOnlyList<string> WarningLines => throw new NotImplementedException();

    public static IReadOnlyList<string> RequestLines => throw new NotImplementedException();

    public static byte[] ExitPacket => throw new NotImplementedException();

    public ExitStep Click() => throw new NotImplementedException();

    public bool ServerEnded(int reasonCode) => throw new NotImplementedException();

    public void Advance(TimeSpan elapsed) => throw new NotImplementedException();
}

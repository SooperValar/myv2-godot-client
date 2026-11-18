using System;

namespace TheLastGate.Windows;

public sealed class PlannerWindow
{
    public bool IsOpen => throw new NotImplementedException();

    public bool AdvancedOpen { get; set; }

    public bool Toggle() => throw new NotImplementedException();

    public bool Close() => throw new NotImplementedException();

    public static string Header(int pointsLeft, int planned) => throw new NotImplementedException();

    public static byte[] MetaRequest() => throw new NotImplementedException();
}

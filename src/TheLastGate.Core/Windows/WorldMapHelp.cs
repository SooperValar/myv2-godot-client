using System;
using System.Collections.Generic;

namespace TheLastGate.Windows;

public sealed record WorldMapHelpPage(string Title, IReadOnlyList<string> Lines);

public static class WorldMapHelp
{
    public static IReadOnlyList<WorldMapHelpPage> Pages => throw new NotImplementedException();
}

using System;
using System.Collections.Generic;

namespace TheLastGate.Windows;

public static class TutorialPages
{
    public const int Tutorials = 6;
    public const int Pages = 3;
    public const int LinesPerPage = 12;

    public static IReadOnlyList<string> Text(int tutorial, int page) => throw new NotImplementedException();

    public static int Picture(int tutorial, int page) => throw new NotImplementedException();
}

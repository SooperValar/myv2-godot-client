using System;

namespace TheLastGate.Windows;

// The page stays where it was between visits, as in the current client.
public sealed class DepotPage
{
    public const int Pages = 8;

    public int Current { get; private set; }

    public void Show(int page) => Current = Math.Clamp(page, 0, Pages - 1);

    public void Turn(int pages) => Show(Current + pages);
}

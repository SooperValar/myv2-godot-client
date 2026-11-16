using TheLastGate.Cursors;

namespace TheLastGate.Items;

public static class HeldItem
{
    public static bool IsHighlighted(MouseCursor cursor) =>
        cursor is MouseCursor.Drop or MouseCursor.Swap or MouseCursor.Use;
}

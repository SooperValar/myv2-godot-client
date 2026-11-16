using System;
using TheLastGate.Cursors;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class TrashCan
{
    public const string Hint = "Dispose of items under your cursor here.";

    public static byte[]? Packet(MapButton button, int selectedCharacter) => throw new NotImplementedException();

    public static MouseCursor Cursor(bool holdingItem) => throw new NotImplementedException();
}

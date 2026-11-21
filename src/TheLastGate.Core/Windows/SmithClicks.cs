using System.Collections.Generic;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Windows;

public static class SmithClicks
{
    public const int Slots = 4;
    public const int FirstButton = 4;
    public const int LastButton = 6;

    private const int MiddleSlot = 1;
    private const int WhetstoneSprite = 17356;
    private const int ClaystoneSprite = 17357;

    // A claystone in the middle needs a second material slot below it.
    public static bool UsesTwoMaterials(IReadOnlyList<ItemSlot> items) => items[MiddleSlot].Sprite == ClaystoneSprite;

    // A whetstone in the middle brings the two outer buttons into play.
    public static bool ShowsOuterButtons(IReadOnlyList<ItemSlot> items) =>
        items[MiddleSlot].Sprite == WhetstoneSprite;

    // A plain left click does nothing: placing needs Shift alone, taking back Ctrl or Alt alone.
    public static byte[]? ItemPacket(MapButton button, MapKeys keys, int shopNumber, int slot) => (button, keys) switch
    {
        (MapButton.Left, MapKeys.Shift) => SmithPackets.Place(shopNumber, slot),
        (MapButton.Left, MapKeys.Ctrl or MapKeys.Alt) => SmithPackets.TakeBack(shopNumber, slot),
        (MapButton.Right, _) => SmithPackets.Check(shopNumber, slot),
        _ => null,
    };

    public static byte[]? ButtonPacket(MapButton button, int shopNumber, int buttonNumber) => button switch
    {
        MapButton.Left => SmithPackets.Press(shopNumber, buttonNumber),
        MapButton.Right => SmithPackets.Check(shopNumber, buttonNumber),
        _ => null,
    };
}

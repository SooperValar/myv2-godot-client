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
    private const int TakeBackOffset = 16;
    private const int CheckOffset = 8;

    // A claystone in the middle needs a second material slot below it.
    public static bool UsesTwoMaterials(IReadOnlyList<ItemSlot> items) => items[MiddleSlot].Sprite == ClaystoneSprite;

    // A whetstone in the middle brings the two outer buttons into play.
    public static bool ShowsOuterButtons(IReadOnlyList<ItemSlot> items) =>
        items[MiddleSlot].Sprite == WhetstoneSprite;

    // A plain left click does nothing: placing needs Shift alone, taking back Ctrl or Alt alone.
    public static byte[]? ItemPacket(MapButton button, MapKeys keys, int shopNumber, int slot)
    {
        var value = (button, keys) switch
        {
            (MapButton.Left, MapKeys.Shift) => slot,
            (MapButton.Left, MapKeys.Ctrl or MapKeys.Alt) => slot + TakeBackOffset,
            (MapButton.Right, _) => slot + CheckOffset,
            _ => (int?)null,
        };
        return value is { } smithValue ? Smith(shopNumber, smithValue) : null;
    }

    public static byte[]? ButtonPacket(MapButton button, int shopNumber, int buttonNumber) => button switch
    {
        MapButton.Left => Smith(shopNumber, buttonNumber),
        MapButton.Right => Smith(shopNumber, buttonNumber + CheckOffset),
        _ => null,
    };

    private static byte[] Smith(int shopNumber, int value) =>
        ClientPackets.ShortAndInt(ClientCommand.Smith, shopNumber, value);
}

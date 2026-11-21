using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Windows;

public static class DepotClicks
{
    public const int Cells = DepotPackets.PageSlots;

    public static byte[]? Packet(MapButton button, int shopNumber, int page, int cell)
    {
        var slot = page * Cells + cell;
        return button switch
        {
            MapButton.Left => DepotPackets.Take(shopNumber, slot),
            MapButton.Right => DepotPackets.Describe(shopNumber, slot),
            _ => null,
        };
    }
}

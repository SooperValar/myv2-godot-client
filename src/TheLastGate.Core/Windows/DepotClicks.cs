using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Windows;

public static class DepotClicks
{
    public const int Cells = 64;

    private const int DescribeOffset = DepotPage.Pages * Cells;

    public static byte[]? Packet(MapButton button, int shopNumber, int page, int cell)
    {
        var slot = page * Cells + cell;
        var value = button switch
        {
            MapButton.Left => slot,
            MapButton.Right => slot + DescribeOffset,
            _ => (int?)null,
        };
        return value is { } shopValue ? ClientPackets.ShortAndInt(ClientCommand.Shop, shopNumber, shopValue) : null;
    }
}

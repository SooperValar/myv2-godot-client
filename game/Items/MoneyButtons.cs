using System.Globalization;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class MoneyButtons
{
    private const int SilverPerGold = 100;

    public static byte[]? Packet(int silver, MapButton button, int selectedCharacter) =>
        button == MapButton.Left ? InventoryPackets.For(InventoryAction.TakeMoney, silver, selectedCharacter) : null;

    public static string Hint(int silver) =>
        string.Format(CultureInfo.InvariantCulture, "{0:N0} gold coins.", silver / SilverPerGold);
}

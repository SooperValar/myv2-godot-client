using System.Collections.Generic;
using System.Linq;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Windows;

// A grave fills the shop window's grid in two pages: the backpack, then what was worn and held, and the gold.
public static class GravePages
{
    public const int Pages = 2;

    // A cell past the end of its page shows nothing.
    public const int NoSlot = CharacterLook.ShopSlots;

    private const int BackpackSlots = PlayerState.InventorySlots;
    private const int SecondPageSlots = CharacterLook.ShopSlots - BackpackSlots;

    public static int SlotFor(int page, int cell) => page == 0
        ? cell < BackpackSlots ? cell : NoSlot
        : cell < SecondPageSlots ? BackpackSlots + cell : NoSlot;

    public static int SecondPageCount(IReadOnlyList<ShopItem> items) =>
        items.Skip(BackpackSlots).Count(item => item.Sprite != 0);

    public static string ButtonText(int page, int secondPageCount) =>
        page == 0 ? $"Worn ({secondPageCount}) >" : "< Backpack";

    public static byte[]? Packet(MapButton button, MapKeys keys, int graveNumber, int slot) =>
        slot == NoSlot || (keys & (MapKeys.Ctrl | MapKeys.Alt)) != 0
            ? null
            : button switch
            {
                MapButton.Left => ShopPackets.Buy(graveNumber, slot),
                MapButton.Right => ShopPackets.DescribeInGrave(graveNumber, slot),
                _ => null,
            };
}

// The page shown stays as it is while the window is redrawn after each take, and starts again on the backpack for
// another grave.
public sealed class GravePage
{
    private int? graveNumber;

    public int Current { get; private set; }

    public int For(int number)
    {
        if (graveNumber != number)
        {
            graveNumber = number;
            Current = 0;
        }

        return Current;
    }

    public void Turn() => Current = (Current + 1) % GravePages.Pages;
}

using TheLastGate.State;

namespace TheLastGate.Items;

// The server's tier and corner marks for one slot: a letter top left (a spell scroll's strength) and a number top
// right (a stack's count, a scroll's charges).
public readonly record struct ItemMark(int Tier, char Letter, int? Number)
{
    private const byte NoNumber = 255;

    public static ItemMark Carried(ItemTiers tiers, int inventorySlot) =>
        From(tiers.CarriedTiers, tiers.CarriedMarks, inventorySlot);

    public static ItemMark Worn(ItemTiers tiers, int wornSlot) =>
        From(tiers.CarriedTiers, tiers.CarriedMarks, ItemTiers.WornStart + wornSlot);

    public static ItemMark Shop(ItemTiers tiers, int shopSlot) => From(tiers.ShopTiers, tiers.ShopMarks, shopSlot);

    private static ItemMark From(byte[] tierBytes, byte[] markBytes, int slot)
    {
        if (slot < 0 || slot >= tierBytes.Length || slot * 2 + 1 >= markBytes.Length)
        {
            return default;
        }

        var number = markBytes[slot * 2 + 1];
        return new ItemMark(tierBytes[slot], (char)markBytes[slot * 2], number == NoNumber ? null : number);
    }
}

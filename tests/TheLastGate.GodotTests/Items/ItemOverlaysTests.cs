using GdUnit4;
using TheLastGate.Items;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class ItemOverlaysTests
{
    private const int Sword = 700;

    [TestCase]
    public void Worn_AddsNothingToAPlainItem()
    {
        AssertThat(ItemOverlays.Worn(new ItemSlot(Sword, (int)ItemPlacement.Weapon, 0, 0))).IsEmpty();
    }

    [TestCase]
    public void Worn_AddsSoulstoneTalismanAndCorruptionInThatOrder()
    {
        var placement = ItemPlacement.Weapon | ItemPlacement.Corrupted | ItemPlacement.Enchanted
            | ItemPlacement.Soulstoned;

        AssertThat(ItemOverlays.Worn(new ItemSlot(Sword, (int)placement, 0, 0)))
            .ContainsExactly(new ItemOverlay(4496, 0), new ItemOverlay(4497, 0), new ItemOverlay(6881, 0));
    }

    [TestCase]
    public void Worn_ReadsTheEnchantedBitFromANegativeSixteenBitValue()
    {
        AssertThat(ItemOverlays.Worn(new ItemSlot(Sword, short.MinValue, 0, 0)))
            .ContainsExactly(new ItemOverlay(4497, 0));
    }

    [TestCase(1, 4001)]
    [TestCase(3, 4003)]
    [TestCase(10, 4010)]
    public void Worn_ShowsStacksOfOneToTen(int stack, int overlay)
    {
        AssertThat(ItemOverlays.Worn(new ItemSlot(Sword, 0, (byte)stack, 0)))
            .ContainsExactly(new ItemOverlay(overlay, 0));
    }

    [TestCase(0)]
    [TestCase(11)]
    public void Worn_ShowsNoStackOutsideOneToTen(int stack)
    {
        AssertThat(ItemOverlays.Worn(new ItemSlot(Sword, 0, (byte)stack, 0))).IsEmpty();
    }

    [TestCase]
    public void Worn_AddsNothingToAnEmptySlot()
    {
        AssertThat(ItemOverlays.Worn(new ItemSlot(0, (int)ItemPlacement.Soulstoned, 3, 0))).IsEmpty();
    }

    [TestCase]
    public void Carried_AddsSoulstoneTalismanAndCorruptionFromTheLockFlags()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(Sword, 0, 0, 2 | 4 | 8)))
            .ContainsExactly(new ItemOverlay(4496, 0), new ItemOverlay(4497, 0), new ItemOverlay(6881, 0));
    }

    [TestCase]
    public void Carried_IgnoresTheLockedFlag()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(Sword, 0, 0, 1))).IsEmpty();
    }

    [TestCase]
    public void Carried_ShowsACatalystsKind()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(16971, 12, 0, 0))).ContainsExactly(new ItemOverlay(6999 + 12, 0));
    }

    [TestCase]
    public void Carried_ShowsACorruptersTwoHexDigitsBesideItsMark()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(6880, 0x3A, 0, 0)))
            .ContainsExactly(new ItemOverlay(7799, 0), new ItemOverlay(7800 + 3, 5), new ItemOverlay(7800 + 10, 10));
    }

    [TestCase]
    public void Carried_ShowsNoKindForOtherItemsOrAZeroValue()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(Sword, 12, 0, 0))).IsEmpty();
        AssertThat(ItemOverlays.Carried(new ItemSlot(16971, 0, 0, 0))).IsEmpty();
    }

    [TestCase]
    public void Carried_PutsTheStackLast()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(Sword, 0, 4, 2)))
            .ContainsExactly(new ItemOverlay(4496, 0), new ItemOverlay(4004, 0));
    }

    [TestCase]
    public void Carried_AddsNothingToAnEmptySlot()
    {
        AssertThat(ItemOverlays.Carried(new ItemSlot(0, 12, 4, 2))).IsEmpty();
    }

    [TestCase]
    public void Shop_OverlaysSoulstoneAndTalismanMarks()
    {
        AssertThat(ItemOverlays.Shop(new ShopItem(1000, 10, true, true, 0)))
            .ContainsExactly(new ItemOverlay(4496, 0), new ItemOverlay(4497, 0));
    }

    [TestCase]
    public void Shop_OverlaysACatalystsKind()
    {
        AssertThat(ItemOverlays.Shop(new ShopItem(16971, 10, false, false, 3)))
            .ContainsExactly(new ItemOverlay(6999 + 3, 0));
    }

    [TestCase]
    public void Shop_OverlaysACorruptersNumber()
    {
        AssertThat(ItemOverlays.Shop(new ShopItem(6880, 10, false, false, 0x25)))
            .ContainsExactly(new ItemOverlay(7799, 0), new ItemOverlay(7800 + 2, 5), new ItemOverlay(7800 + 5, 10));
    }

    [TestCase]
    public void Shop_LeavesAnEmptySlotBare()
    {
        AssertThat(ItemOverlays.Shop(new ShopItem(0, 0, true, true, 3))).IsEmpty();
    }
}

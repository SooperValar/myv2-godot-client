using TheLastGate.Items;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ItemOverlaysTests
{
    private const int Sword = 700;

    [Fact]
    public void Worn_AddsNothingToAPlainItem()
    {
        Assert.Empty(ItemOverlays.Worn(new ItemSlot(Sword, (int)ItemPlacement.Weapon, 0, 0)));
    }

    [Fact]
    public void Worn_AddsSoulstoneTalismanAndCorruptionInThatOrder()
    {
        var placement = ItemPlacement.Weapon | ItemPlacement.Corrupted | ItemPlacement.Enchanted
            | ItemPlacement.Soulstoned;

        Assert.Equal(
            [new ItemOverlay(4496, 0), new ItemOverlay(4497, 0), new ItemOverlay(6881, 0)],
            ItemOverlays.Worn(new ItemSlot(Sword, (int)placement, 0, 0)));
    }

    [Fact]
    public void Worn_ReadsTheEnchantedBitFromANegativeSixteenBitValue()
    {
        Assert.Equal([new ItemOverlay(4497, 0)], ItemOverlays.Worn(new ItemSlot(Sword, short.MinValue, 0, 0)));
    }

    [Theory]
    [InlineData(1, 4001)]
    [InlineData(3, 4003)]
    [InlineData(10, 4010)]
    public void Worn_ShowsStacksOfOneToTen(byte stack, int overlay)
    {
        Assert.Equal([new ItemOverlay(overlay, 0)], ItemOverlays.Worn(new ItemSlot(Sword, 0, stack, 0)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Worn_ShowsNoStackOutsideOneToTen(byte stack)
    {
        Assert.Empty(ItemOverlays.Worn(new ItemSlot(Sword, 0, stack, 0)));
    }

    [Fact]
    public void Worn_AddsNothingToAnEmptySlot()
    {
        Assert.Empty(ItemOverlays.Worn(new ItemSlot(0, (int)ItemPlacement.Soulstoned, 3, 0)));
    }

    [Fact]
    public void Carried_AddsSoulstoneTalismanAndCorruptionFromTheLockFlags()
    {
        Assert.Equal(
            [new ItemOverlay(4496, 0), new ItemOverlay(4497, 0), new ItemOverlay(6881, 0)],
            ItemOverlays.Carried(new ItemSlot(Sword, 0, 0, 2 | 4 | 8)));
    }

    [Fact]
    public void Carried_IgnoresTheLockedFlag()
    {
        Assert.Empty(ItemOverlays.Carried(new ItemSlot(Sword, 0, 0, 1)));
    }

    [Fact]
    public void Carried_ShowsACatalystsKind()
    {
        Assert.Equal([new ItemOverlay(6999 + 12, 0)], ItemOverlays.Carried(new ItemSlot(16971, 12, 0, 0)));
    }

    [Fact]
    public void Carried_ShowsACorruptersTwoHexDigitsBesideItsMark()
    {
        Assert.Equal(
            [new ItemOverlay(7799, 0), new ItemOverlay(7800 + 3, 5), new ItemOverlay(7800 + 10, 10)],
            ItemOverlays.Carried(new ItemSlot(6880, 0x3A, 0, 0)));
    }

    [Fact]
    public void Carried_ShowsNoKindForOtherItemsOrAZeroValue()
    {
        Assert.Empty(ItemOverlays.Carried(new ItemSlot(Sword, 12, 0, 0)));
        Assert.Empty(ItemOverlays.Carried(new ItemSlot(16971, 0, 0, 0)));
    }

    [Fact]
    public void Carried_PutsTheStackLast()
    {
        Assert.Equal(
            [new ItemOverlay(4496, 0), new ItemOverlay(4004, 0)],
            ItemOverlays.Carried(new ItemSlot(Sword, 0, 4, 2)));
    }

    [Fact]
    public void Carried_AddsNothingToAnEmptySlot()
    {
        Assert.Empty(ItemOverlays.Carried(new ItemSlot(0, 12, 4, 2)));
    }
}

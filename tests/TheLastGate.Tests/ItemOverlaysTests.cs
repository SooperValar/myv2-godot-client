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

        Assert.Equal([4496, 4497, 6881], ItemOverlays.Worn(new ItemSlot(Sword, (int)placement, 0, 0)));
    }

    [Fact]
    public void Worn_ReadsTheEnchantedBitFromANegativeSixteenBitValue()
    {
        Assert.Equal([4497], ItemOverlays.Worn(new ItemSlot(Sword, short.MinValue, 0, 0)));
    }

    [Theory]
    [InlineData(1, 4001)]
    [InlineData(3, 4003)]
    [InlineData(10, 4010)]
    public void Worn_ShowsStacksOfOneToTen(byte stack, int overlay)
    {
        Assert.Equal([overlay], ItemOverlays.Worn(new ItemSlot(Sword, 0, stack, 0)));
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
}

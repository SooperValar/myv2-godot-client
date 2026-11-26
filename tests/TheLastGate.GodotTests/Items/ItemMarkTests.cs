using GdUnit4;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class ItemMarkTests
{
    private const byte NoNumber = 255;

    [TestCase]
    public void Carried_ReadsTheSlotsTierLetterAndNumber()
    {
        var tiers = new ItemTiers();
        tiers.CarriedTiers[5] = 3;
        tiers.CarriedMarks[10] = (byte)'S';
        tiers.CarriedMarks[11] = 12;

        AssertThat(ItemMark.Carried(tiers, 5)).IsEqual(new ItemMark(3, 'S', 12));
    }

    [TestCase]
    public void Worn_CountsFromTheSixtiethCarriedSlot()
    {
        var tiers = new ItemTiers();
        tiers.CarriedTiers[62] = 4;
        tiers.CarriedMarks[125] = NoNumber;

        AssertThat(ItemMark.Worn(tiers, 2)).IsEqual(new ItemMark(4, '\0', null));
    }

    [TestCase]
    public void Shop_ReadsAStackCount()
    {
        var tiers = new ItemTiers();
        tiers.ShopMarks[1] = 7;

        AssertThat(ItemMark.Shop(tiers, 0)).IsEqual(new ItemMark(0, '\0', 7));
    }

    [TestCase]
    public void SlotsPastTheListHaveNoMark()
    {
        AssertThat(ItemMark.Shop(new ItemTiers(), GravePages.NoSlot)).IsEqual(default(ItemMark));
    }
}

using GdUnit4;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class GravePagesTests
{
    private const int GraveNumber = 900;

    [TestCase(0, 0, 0)]
    [TestCase(0, 59, 59)]
    [TestCase(0, 60, GravePages.NoSlot)]
    [TestCase(1, 0, 60)]
    [TestCase(1, 20, 80)]
    [TestCase(1, 21, 81)]
    [TestCase(1, 22, GravePages.NoSlot)]
    public void SlotFor_PutsTheBackpackFirstAndTheRestOnPageTwo(int page, int cell, int slot)
    {
        AssertThat(GravePages.SlotFor(page, cell)).IsEqual(slot);
    }

    [TestCase]
    public void SecondPageCount_CountsWornHeldAndGold()
    {
        var items = new ShopItem[CharacterLook.ShopSlots];
        items[10] = new ShopItem(500, 0, false, false, 0);
        items[60] = new ShopItem(501, 0, false, false, 0);
        items[81] = new ShopItem(37, 0, false, false, 0);

        AssertThat(GravePages.SecondPageCount(items)).IsEqual(2);
        AssertThat(GravePages.ButtonText(0, 2)).IsEqual("Worn (2) >");
        AssertThat(GravePages.ButtonText(1, 2)).IsEqual("< Backpack");
    }

    [TestCase]
    public void Packet_TakesOnALeftClickAndDescribesOnARightClick()
    {
        AssertThat(GravePages.Packet(MapButton.Left, MapKeys.None, GraveNumber, 70))
            .IsEqual(ShopPackets.Buy(GraveNumber, 70));
        AssertThat(GravePages.Packet(MapButton.Right, MapKeys.None, GraveNumber, 70))
            .IsEqual(ShopPackets.DescribeInGrave(GraveNumber, 70));
        AssertThat(GravePages.Packet(MapButton.Left, MapKeys.Ctrl, GraveNumber, 70)).IsNull();
        AssertThat(GravePages.Packet(MapButton.Left, MapKeys.None, GraveNumber, GravePages.NoSlot)).IsNull();
    }

    [TestCase]
    public void GravePage_StaysWhileTheSameGraveIsRedrawnAndStartsOverForAnother()
    {
        var page = new GravePage();
        page.For(GraveNumber);
        page.Turn();

        AssertThat(page.For(GraveNumber)).IsEqual(1);
        AssertThat(page.For(GraveNumber + 1)).IsEqual(0);
    }
}

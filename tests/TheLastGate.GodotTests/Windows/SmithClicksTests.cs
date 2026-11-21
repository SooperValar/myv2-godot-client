using GdUnit4;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class SmithClicksTests
{
    private const int SmithNumber = 1302;
    private const int Whetstone = 17356;
    private const int Claystone = 17357;

    [TestCase(MapKeys.Shift, 2)]
    [TestCase(MapKeys.Ctrl, 18)]
    [TestCase(MapKeys.Alt, 18)]
    public void ItemPacket_PlacesWithShiftAndTakesBackWithControlOrAlt(MapKeys keys, int value)
    {
        AssertThat(SmithClicks.ItemPacket(MapButton.Left, keys, SmithNumber, 2)).IsEqual(Smith(value));
    }

    [TestCase(MapKeys.None)]
    [TestCase(MapKeys.Ctrl | MapKeys.Alt)]
    [TestCase(MapKeys.Ctrl | MapKeys.Shift)]
    [TestCase(MapKeys.Shift | MapKeys.Alt)]
    public void ItemPacket_DoesNothingOnAPlainOrMixedLeftClick(MapKeys keys)
    {
        AssertThat(SmithClicks.ItemPacket(MapButton.Left, keys, SmithNumber, 2)).IsNull();
    }

    [TestCase(MapKeys.None)]
    [TestCase(MapKeys.Shift)]
    [TestCase(MapKeys.Ctrl | MapKeys.Alt)]
    public void ItemPacket_ChecksTheItemOnARightClick(MapKeys keys)
    {
        AssertThat(SmithClicks.ItemPacket(MapButton.Right, keys, SmithNumber, 2)).IsEqual(Smith(10));
    }

    [TestCase]
    public void ItemPacket_SendsNothingForOtherButtons()
    {
        AssertThat(SmithClicks.ItemPacket(MapButton.None, MapKeys.Shift, SmithNumber, 2)).IsNull();
    }

    [TestCase(MapButton.Left, 4, 4)]
    [TestCase(MapButton.Left, 5, 5)]
    [TestCase(MapButton.Right, 6, 14)]
    public void ButtonPacket_UsesTheButtonOrChecksItOnARightClick(MapButton button, int buttonNumber, int value)
    {
        AssertThat(SmithClicks.ButtonPacket(button, SmithNumber, buttonNumber)).IsEqual(Smith(value));
    }

    [TestCase]
    public void ButtonPacket_SendsNothingForOtherButtons()
    {
        AssertThat(SmithClicks.ButtonPacket(MapButton.None, SmithNumber, 5)).IsNull();
    }

    [TestCase(Claystone, true, false)]
    [TestCase(Whetstone, false, true)]
    [TestCase(1000, false, false)]
    [TestCase(0, false, false)]
    public void TheMiddleItem_ChoosesTheLayout(int middleSprite, bool twoMaterials, bool outerButtons)
    {
        ItemSlot[] items = [default, new(middleSprite, 0, 0, 0), default, default];

        AssertThat(SmithClicks.UsesTwoMaterials(items)).IsEqual(twoMaterials);
        AssertThat(SmithClicks.ShowsOuterButtons(items)).IsEqual(outerButtons);
    }

    private static byte[] Smith(int value) => ClientPackets.ShortAndInt(ClientCommand.Smith, SmithNumber, value);
}

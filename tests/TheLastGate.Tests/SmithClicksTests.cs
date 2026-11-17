using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class SmithClicksTests
{
    private const int SmithNumber = 1302;
    private const int Whetstone = 17356;
    private const int Claystone = 17357;

    [Theory]
    [InlineData(MapKeys.Shift, 2)]
    [InlineData(MapKeys.Ctrl, 18)]
    [InlineData(MapKeys.Alt, 18)]
    public void ItemPacket_PlacesWithShiftAndTakesBackWithControlOrAlt(MapKeys keys, int value)
    {
        Assert.Equal(Smith(value), SmithClicks.ItemPacket(MapButton.Left, keys, SmithNumber, 2));
    }

    [Theory]
    [InlineData(MapKeys.None)]
    [InlineData(MapKeys.Ctrl | MapKeys.Alt)]
    [InlineData(MapKeys.Ctrl | MapKeys.Shift)]
    [InlineData(MapKeys.Shift | MapKeys.Alt)]
    public void ItemPacket_DoesNothingOnAPlainOrMixedLeftClick(MapKeys keys)
    {
        Assert.Null(SmithClicks.ItemPacket(MapButton.Left, keys, SmithNumber, 2));
    }

    [Theory]
    [InlineData(MapKeys.None)]
    [InlineData(MapKeys.Shift)]
    [InlineData(MapKeys.Ctrl | MapKeys.Alt)]
    public void ItemPacket_ChecksTheItemOnARightClick(MapKeys keys)
    {
        Assert.Equal(Smith(10), SmithClicks.ItemPacket(MapButton.Right, keys, SmithNumber, 2));
    }

    [Fact]
    public void ItemPacket_SendsNothingForOtherButtons()
    {
        Assert.Null(SmithClicks.ItemPacket(MapButton.None, MapKeys.Shift, SmithNumber, 2));
    }

    [Theory]
    [InlineData(MapButton.Left, 4, 4)]
    [InlineData(MapButton.Left, 5, 5)]
    [InlineData(MapButton.Right, 6, 14)]
    public void ButtonPacket_UsesTheButtonOrChecksItOnARightClick(MapButton button, int buttonNumber, int value)
    {
        Assert.Equal(Smith(value), SmithClicks.ButtonPacket(button, SmithNumber, buttonNumber));
    }

    [Fact]
    public void ButtonPacket_SendsNothingForOtherButtons()
    {
        Assert.Null(SmithClicks.ButtonPacket(MapButton.None, SmithNumber, 5));
    }

    [Theory]
    [InlineData(Claystone, true, false)]
    [InlineData(Whetstone, false, true)]
    [InlineData(1000, false, false)]
    [InlineData(0, false, false)]
    public void TheMiddleItem_ChoosesTheLayout(int middleSprite, bool twoMaterials, bool outerButtons)
    {
        ItemSlot[] items = [default, new(middleSprite, 0, 0, 0), default, default];

        Assert.Equal(twoMaterials, SmithClicks.UsesTwoMaterials(items));
        Assert.Equal(outerButtons, SmithClicks.ShowsOuterButtons(items));
    }

    private static byte[] Smith(int value) => ClientPackets.ShortAndInt(ClientCommand.Smith, SmithNumber, value);
}

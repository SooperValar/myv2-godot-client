using GdUnit4;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class TrashTests
{
    private const int TrashNumber = 0x4000 | 12;

    private static readonly TimeSpan Start = TimeSpan.FromSeconds(100);

    [TestCase]
    public void TrashButton_FirstClickOnlyArms()
    {
        var button = new TrashButton();

        AssertThat(button.Click(Start, TrashNumber, 3)).IsNull();
        AssertThat(button.IsArmed(Start + TimeSpan.FromSeconds(1))).IsTrue();
    }

    [TestCase]
    public void TrashButton_SecondClickWhileArmedEmptiesTheTrash()
    {
        var button = new TrashButton();
        button.Click(Start, TrashNumber, 3);

        var packet = button.Click(Start + TimeSpan.FromSeconds(5), TrashNumber, 3);

        AssertThat(packet).IsEqual(ShopPackets.EmptyTrash(TrashNumber));
        AssertThat(button.IsArmed(Start + TimeSpan.FromSeconds(5))).IsFalse();
    }

    [TestCase]
    public void TrashButton_DisarmsAfterTenSeconds()
    {
        var button = new TrashButton();
        button.Click(Start, TrashNumber, 3);

        var later = Start + TrashButton.ArmedFor;

        AssertThat(button.IsArmed(later)).IsFalse();
        AssertThat(button.Click(later, TrashNumber, 3)).IsNull();
    }

    [TestCase]
    public void TrashButton_DoesNothingWithAnEmptyTrash()
    {
        var button = new TrashButton();

        AssertThat(button.Click(Start, TrashNumber, 0)).IsNull();
        AssertThat(button.IsArmed(Start)).IsFalse();
    }

    [TestCase]
    public void TrashWatch_SendsCloseOnceWhenTheTrashGoesAway()
    {
        var watch = new TrashWatch();
        watch.Follow(ShopKind.Trash, TrashNumber);

        AssertThat(watch.Follow(null, 0)).IsEqual(ShopPackets.CloseTrash(TrashNumber));
        AssertThat(watch.Follow(null, 0)).IsNull();
    }

    [TestCase]
    public void TrashWatch_SendsCloseWhenAnotherShopReplacesTheTrash()
    {
        var watch = new TrashWatch();
        watch.Follow(ShopKind.Trash, TrashNumber);

        AssertThat(watch.Follow(ShopKind.Merchant, 1201)).IsEqual(ShopPackets.CloseTrash(TrashNumber));
    }

    [TestCase]
    public void TrashWatch_StaysQuietWhileTheTrashIsOpen()
    {
        var watch = new TrashWatch();

        AssertThat(watch.Follow(ShopKind.Trash, TrashNumber)).IsNull();
        AssertThat(watch.Follow(ShopKind.Trash, TrashNumber)).IsNull();
    }

    [TestCase]
    public void ShiftClickInTheBackpack_PutsTheItemInTheOpenTrash()
    {
        var packets = InventoryClicks.Packets(
            7,
            new ItemSlot(1500, 0, 0, 0),
            false,
            MapButton.Left,
            MapKeys.Shift,
            null,
            0,
            TrashNumber);

        AssertThat(packets).ContainsExactly(ShopPackets.PutInTrash(TrashNumber, 7));
    }

    [TestCase]
    public void ShiftClickInTheBackpack_ShowsTheTrashCursorWhileTheTrashIsOpen()
    {
        var cursor = InventoryClicks.Cursor(new ItemSlot(1500, 0, 0, 0), false, MapKeys.Shift, true);

        AssertThat(cursor).IsEqual(Cursors.MouseCursor.Trash);
    }

    [TestCase(MapButton.Left, MapKeys.None, true)]
    [TestCase(MapButton.Left, MapKeys.Ctrl, false)]
    [TestCase(MapButton.Right, MapKeys.None, false)]
    public void TrashCells_OnlyAPlainClickTakesAnItemBack(MapButton button, MapKeys keys, bool sends)
    {
        var packet = ShopClicks.TrashPacket(button, keys, TrashNumber, 4);

        AssertThat(packet != null).IsEqual(sends);
    }
}

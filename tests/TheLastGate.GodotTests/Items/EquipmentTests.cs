using GdUnit4;
using TheLastGate.Items;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class EquipmentTests
{
    private const int AnyItem = 700;

    private static readonly ItemSlot NothingHeld = new(0, 0, 0, 0);

    [TestCase]
    public void BlockedSlots_BlocksNothingWithEmptyHands()
    {
        AssertThat(Equipment.BlockedSlots(NothingHeld, Worn())).IsEmpty();
    }

    [TestCase]
    public void BlockedSlots_LeavesOnlyTheHeldItemsSlotOpen()
    {
        var blocked = Equipment.BlockedSlots(Held(ItemPlacement.Head), Worn());

        AssertThat(blocked.Count).IsEqual(Equipment.ShownSlots - 1);
        AssertThat(blocked).NotContains(WornSlot.Head);
    }

    [TestCase(ItemPlacement.Charm, WornSlot.Charm, WornSlot.SecondCharm)]
    [TestCase(ItemPlacement.Ring, WornSlot.LeftRing, WornSlot.RightRing)]
    public void BlockedSlots_OpensBothSlotsOfAPair(ItemPlacement placement, WornSlot first, WornSlot second)
    {
        var blocked = Equipment.BlockedSlots(Held(placement), Worn());

        AssertThat(blocked.Count).IsEqual(Equipment.ShownSlots - 2);
        AssertThat(blocked).NotContains(first);
        AssertThat(blocked).NotContains(second);
    }

    [TestCase]
    public void BlockedSlots_BlocksTheOffHandBehindATwoHandedWeapon()
    {
        var worn = Worn((WornSlot.MainHand, ItemPlacement.Weapon | ItemPlacement.TwoHanded));

        AssertThat(Equipment.BlockedSlots(NothingHeld, worn)).ContainsExactly(WornSlot.OffHand);
    }

    [TestCase]
    public void BlockedSlots_BlocksTheMainHandForAHeldTwoHandedWeaponWhileAShieldIsWorn()
    {
        var worn = Worn((WornSlot.OffHand, ItemPlacement.Shield));

        var blocked = Equipment.BlockedSlots(Held(ItemPlacement.Weapon | ItemPlacement.TwoHanded), worn);

        AssertThat(blocked).Contains(WornSlot.MainHand);
    }

    [TestCase]
    public void BlockedSlots_LeavesTheMainHandOpenForAOneHandedWeaponWhileAShieldIsWorn()
    {
        var worn = Worn((WornSlot.OffHand, ItemPlacement.Shield));

        var blocked = Equipment.BlockedSlots(Held(ItemPlacement.Weapon), worn);

        AssertThat(blocked).NotContains(WornSlot.MainHand);
    }

    [TestCase(WornSlot.LeftRing, WornSlot.RightRing)]
    [TestCase(WornSlot.RightRing, WornSlot.LeftRing)]
    public void BlockedSlots_BlocksTheOtherRingBehindATwoHandedRing(WornSlot wornRing, WornSlot otherRing)
    {
        var worn = Worn((wornRing, ItemPlacement.Ring | ItemPlacement.TwoHanded));

        AssertThat(Equipment.BlockedSlots(NothingHeld, worn)).ContainsExactly(otherRing);
    }

    private static ItemSlot Held(ItemPlacement placement) => new(AnyItem, (int)placement, 0, 0);

    private static ItemSlot[] Worn(params (WornSlot Slot, ItemPlacement Placement)[] items)
    {
        var worn = new ItemSlot[PlayerState.WornSlots];
        foreach (var (slot, placement) in items)
        {
            worn[(int)slot] = new ItemSlot(AnyItem, (int)placement, 0, 0);
        }

        return worn;
    }
}

using TheLastGate.Items;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class EquipmentTests
{
    private const int AnyItem = 700;

    private static readonly ItemSlot NothingHeld = new(0, 0, 0, 0);

    [Fact]
    public void BlockedSlots_BlocksNothingWithEmptyHands()
    {
        Assert.Empty(Equipment.BlockedSlots(NothingHeld, Worn()));
    }

    [Fact]
    public void BlockedSlots_LeavesOnlyTheHeldItemsSlotOpen()
    {
        var blocked = Equipment.BlockedSlots(Held(ItemPlacement.Head), Worn());

        Assert.Equal(Equipment.ShownSlots - 1, blocked.Count);
        Assert.DoesNotContain(WornSlot.Head, blocked);
    }

    [Theory]
    [InlineData(ItemPlacement.Charm, WornSlot.Charm, WornSlot.SecondCharm)]
    [InlineData(ItemPlacement.Ring, WornSlot.LeftRing, WornSlot.RightRing)]
    public void BlockedSlots_OpensBothSlotsOfAPair(ItemPlacement placement, WornSlot first, WornSlot second)
    {
        var blocked = Equipment.BlockedSlots(Held(placement), Worn());

        Assert.Equal(Equipment.ShownSlots - 2, blocked.Count);
        Assert.DoesNotContain(first, blocked);
        Assert.DoesNotContain(second, blocked);
    }

    [Fact]
    public void BlockedSlots_BlocksTheOffHandBehindATwoHandedWeapon()
    {
        var worn = Worn((WornSlot.MainHand, ItemPlacement.Weapon | ItemPlacement.TwoHanded));

        Assert.Equal([WornSlot.OffHand], Equipment.BlockedSlots(NothingHeld, worn));
    }

    [Fact]
    public void BlockedSlots_BlocksTheMainHandForAHeldTwoHandedWeaponWhileAShieldIsWorn()
    {
        var worn = Worn((WornSlot.OffHand, ItemPlacement.Shield));

        var blocked = Equipment.BlockedSlots(Held(ItemPlacement.Weapon | ItemPlacement.TwoHanded), worn);

        Assert.Contains(WornSlot.MainHand, blocked);
    }

    [Fact]
    public void BlockedSlots_LeavesTheMainHandOpenForAOneHandedWeaponWhileAShieldIsWorn()
    {
        var worn = Worn((WornSlot.OffHand, ItemPlacement.Shield));

        var blocked = Equipment.BlockedSlots(Held(ItemPlacement.Weapon), worn);

        Assert.DoesNotContain(WornSlot.MainHand, blocked);
    }

    [Theory]
    [InlineData(WornSlot.LeftRing, WornSlot.RightRing)]
    [InlineData(WornSlot.RightRing, WornSlot.LeftRing)]
    public void BlockedSlots_BlocksTheOtherRingBehindATwoHandedRing(WornSlot wornRing, WornSlot otherRing)
    {
        var worn = Worn((wornRing, ItemPlacement.Ring | ItemPlacement.TwoHanded));

        Assert.Equal([otherRing], Equipment.BlockedSlots(NothingHeld, worn));
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

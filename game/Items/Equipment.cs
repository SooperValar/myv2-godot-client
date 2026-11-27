using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Items;

public static class Equipment
{
    public const int ShownSlots = 13;

    private static readonly (WornSlot Slot, ItemPlacement Placement)[] SlotPlacements =
    [
        (WornSlot.Head, ItemPlacement.Head),
        (WornSlot.Neck, ItemPlacement.Neck),
        (WornSlot.Body, ItemPlacement.Body),
        (WornSlot.Arms, ItemPlacement.Arms),
        (WornSlot.Belt, ItemPlacement.Belt),
        (WornSlot.Legs, ItemPlacement.Legs),
        (WornSlot.Feet, ItemPlacement.Feet),
        (WornSlot.OffHand, ItemPlacement.Shield),
        (WornSlot.MainHand, ItemPlacement.Weapon),
        (WornSlot.Cloak, ItemPlacement.Cloak),
        (WornSlot.LeftRing, ItemPlacement.Ring),
        (WornSlot.RightRing, ItemPlacement.Ring),
        (WornSlot.SecondCharm, ItemPlacement.Legs),
    ];

    public static IReadOnlySet<WornSlot> BlockedSlots(ItemSlot heldItem, IReadOnlyList<ItemSlot> worn)
    {
        var blocked = new HashSet<WornSlot>();
        var held = (ItemPlacement)heldItem.Properties;
        if (heldItem.Sprite != 0)
        {
            foreach (var (slot, placement) in SlotPlacements)
            {
                if (!held.HasFlag(placement))
                {
                    blocked.Add(slot);
                }
            }
        }

        if (PlacementOf(worn, WornSlot.MainHand).HasFlag(ItemPlacement.TwoHanded))
        {
            blocked.Add(WornSlot.OffHand);
        }

        if (PlacementOf(worn, WornSlot.OffHand) != ItemPlacement.None
            && held.HasFlag(ItemPlacement.Weapon | ItemPlacement.TwoHanded))
        {
            blocked.Add(WornSlot.MainHand);
        }

        if (PlacementOf(worn, WornSlot.LeftRing).HasFlag(ItemPlacement.TwoHanded))
        {
            blocked.Add(WornSlot.RightRing);
        }

        if (PlacementOf(worn, WornSlot.RightRing).HasFlag(ItemPlacement.TwoHanded))
        {
            blocked.Add(WornSlot.LeftRing);
        }

        return blocked;
    }

    private static ItemPlacement PlacementOf(IReadOnlyList<ItemSlot> worn, WornSlot slot) =>
        (ItemPlacement)worn[(int)slot].Properties;
}

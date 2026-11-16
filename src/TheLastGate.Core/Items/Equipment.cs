using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Items;

public static class Equipment
{
    public const int ShownSlots = 13;

    public static IReadOnlySet<WornSlot> BlockedSlots(ItemSlot heldItem, IReadOnlyList<ItemSlot> worn) =>
        throw new NotImplementedException();
}

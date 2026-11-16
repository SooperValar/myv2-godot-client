using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Items;

public static class ItemOverlays
{
    private const int SoulstoneSprite = 4496;
    private const int TalismanSprite = 4497;
    private const int CorruptionSprite = 6881;
    private const int StackSpriteBase = 4000;
    private const int LargestShownStack = 10;

    public static IReadOnlyList<ItemOverlay> Worn(ItemSlot item)
    {
        var overlays = new List<ItemOverlay>();
        if (item.Sprite == 0)
        {
            return overlays;
        }

        var placement = (ItemPlacement)item.Properties;
        if (placement.HasFlag(ItemPlacement.Soulstoned))
        {
            overlays.Add(new ItemOverlay(SoulstoneSprite, 0));
        }

        if (placement.HasFlag(ItemPlacement.Enchanted))
        {
            overlays.Add(new ItemOverlay(TalismanSprite, 0));
        }

        if (placement.HasFlag(ItemPlacement.Corrupted))
        {
            overlays.Add(new ItemOverlay(CorruptionSprite, 0));
        }

        if (item.Stack is > 0 and <= LargestShownStack)
        {
            overlays.Add(new ItemOverlay(StackSpriteBase + item.Stack, 0));
        }

        return overlays;
    }

    public static IReadOnlyList<ItemOverlay> Carried(ItemSlot item) => throw new NotImplementedException();
}

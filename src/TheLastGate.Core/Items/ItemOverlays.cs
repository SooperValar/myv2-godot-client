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

    public static IReadOnlyList<int> Worn(ItemSlot item)
    {
        var overlays = new List<int>();
        if (item.Sprite == 0)
        {
            return overlays;
        }

        var placement = (ItemPlacement)item.Properties;
        if (placement.HasFlag(ItemPlacement.Soulstoned))
        {
            overlays.Add(SoulstoneSprite);
        }

        if (placement.HasFlag(ItemPlacement.Enchanted))
        {
            overlays.Add(TalismanSprite);
        }

        if (placement.HasFlag(ItemPlacement.Corrupted))
        {
            overlays.Add(CorruptionSprite);
        }

        if (item.Stack is > 0 and <= LargestShownStack)
        {
            overlays.Add(StackSpriteBase + item.Stack);
        }

        return overlays;
    }
}

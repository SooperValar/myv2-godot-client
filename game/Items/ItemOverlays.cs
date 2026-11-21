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
    private const int CatalystSprite = 16971;
    private const int CatalystOverlayBase = 6999;
    private const int CorrupterSprite = 6880;
    private const int CorrupterOverlay = 7799;
    private const int CorrupterDigitBase = 7800;
    private const int CorrupterDigitSpacing = 5;
    private const byte StoredSoulstone = 1;
    private const byte StoredTalisman = 2;
    private const byte StoredCorruption = 4;

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

    public static IReadOnlyList<ItemOverlay> Shop(ShopItem item)
    {
        var overlays = new List<ItemOverlay>();
        if (item.Sprite == 0)
        {
            return overlays;
        }

        if (item.IsSoulstone)
        {
            overlays.Add(new ItemOverlay(SoulstoneSprite, 0));
        }

        if (item.IsEnchanted)
        {
            overlays.Add(new ItemOverlay(TalismanSprite, 0));
        }

        AddCatalyst(overlays, item.Sprite, item.Catalyst);
        return overlays;
    }

    // Depot items carry their marks as plain bits rather than the inventory's flags.
    public static IReadOnlyList<ItemOverlay> Stored(DepotItem item)
    {
        var overlays = new List<ItemOverlay>();
        if (item.Sprite == 0)
        {
            return overlays;
        }

        if ((item.Flags & StoredSoulstone) != 0)
        {
            overlays.Add(new ItemOverlay(SoulstoneSprite, 0));
        }

        if ((item.Flags & StoredTalisman) != 0)
        {
            overlays.Add(new ItemOverlay(TalismanSprite, 0));
        }

        if ((item.Flags & StoredCorruption) != 0)
        {
            overlays.Add(new ItemOverlay(CorruptionSprite, 0));
        }

        AddCatalyst(overlays, item.Sprite, item.Catalyst);
        if (item.Stack is > 0 and <= LargestShownStack)
        {
            overlays.Add(new ItemOverlay(StackSpriteBase + item.Stack, 0));
        }

        return overlays;
    }

    public static IReadOnlyList<ItemOverlay> Carried(ItemSlot item)
    {
        var overlays = new List<ItemOverlay>();
        if (item.Sprite == 0)
        {
            return overlays;
        }

        var flags = (CarriedItemFlags)item.Flags;
        if (flags.HasFlag(CarriedItemFlags.Soulstoned))
        {
            overlays.Add(new ItemOverlay(SoulstoneSprite, 0));
        }

        if (flags.HasFlag(CarriedItemFlags.Enchanted))
        {
            overlays.Add(new ItemOverlay(TalismanSprite, 0));
        }

        if (flags.HasFlag(CarriedItemFlags.Corrupted))
        {
            overlays.Add(new ItemOverlay(CorruptionSprite, 0));
        }

        AddCatalyst(overlays, item.Sprite, (uint)item.Properties);

        if (item.Stack is > 0 and <= LargestShownStack)
        {
            overlays.Add(new ItemOverlay(StackSpriteBase + item.Stack, 0));
        }

        return overlays;
    }

    // A catalyst shows its kind; a corrupter shows its number as two digits.
    private static void AddCatalyst(List<ItemOverlay> overlays, int sprite, uint value)
    {
        if (value > 0 && sprite == CatalystSprite)
        {
            overlays.Add(new ItemOverlay(CatalystOverlayBase + (int)value, 0));
        }
        else if (value > 0 && sprite == CorrupterSprite)
        {
            overlays.Add(new ItemOverlay(CorrupterOverlay, 0));
            overlays.Add(new ItemOverlay(CorrupterDigitBase + (int)(value / 16), CorrupterDigitSpacing));
            overlays.Add(new ItemOverlay(CorrupterDigitBase + (int)(value % 16), CorrupterDigitSpacing * 2));
        }
    }
}

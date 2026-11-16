using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Hud;

public static class BuffIcons
{
    private const int FullyLit = 15;
    private const float Gamma = 120;

    private static readonly HashSet<int> DebuffSprites =
    [
        89, 91, 97, 119, 149, 178, 224, 225, 319, 325, 329, 338, 365, 411, 419, 471, 487, 489, 498, 617, 618, 702,
        1009, 1015, 3493, 3494,
    ];

    public static bool IsDebuff(int sprite) =>
        DebuffSprites.Contains(sprite) || sprite is >= 6721 and <= 6745 or >= 6761 and <= 6780 or >= 16860 and <= 16863;

    public static BuffIconLists For(IReadOnlyList<BuffSlot> slots)
    {
        var buffs = new List<BuffIcon>();
        var debuffs = new List<BuffIcon>();
        foreach (var slot in slots)
        {
            if (slot.Sprite > 0)
            {
                (IsDebuff(slot.Sprite) ? debuffs : buffs).Add(new BuffIcon(slot.Sprite, Brightness(slot.Active)));
            }
        }

        return new BuffIconLists(buffs, debuffs);
    }

    private static float Brightness(int active)
    {
        var darkness = FullyLit - Math.Min(FullyLit, active);
        return darkness > 0 ? Gamma / (darkness * darkness + Gamma) : 1f;
    }
}

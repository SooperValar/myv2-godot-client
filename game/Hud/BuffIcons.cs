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

    // For a slot the server sent no timer for; a timer says itself whether its buff is harmful.
    public static bool IsDebuff(int sprite) =>
        DebuffSprites.Contains(sprite) || sprite is >= 6721 and <= 6745 or >= 6761 and <= 6780 or >= 16860 and <= 16863;

    public static BuffIconLists For(IReadOnlyList<BuffSlot> slots, BuffTimers? timers = null)
    {
        var buffs = new List<BuffIcon>();
        var debuffs = new List<BuffIcon>();
        for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
        {
            var slot = slots[slotIndex];
            if (slot.Sprite <= 0)
            {
                continue;
            }

            var timer = TimerFor(timers, slotIndex);
            var harmful = timer?.IsHarmful ?? IsDebuff(slot.Sprite);
            var icon = timer is { } found
                ? new BuffIcon(slot.Sprite, Brightness(slot.Active), found.Name, timers!.SecondsLeft(slotIndex))
                : new BuffIcon(slot.Sprite, Brightness(slot.Active));
            (harmful ? debuffs : buffs).Add(icon);
        }

        return new BuffIconLists(buffs, debuffs);
    }

    private static BuffTimer? TimerFor(BuffTimers? timers, int slotIndex) =>
        timers != null && slotIndex < BuffTimers.Slots && !string.IsNullOrEmpty(timers.Timers[slotIndex].Name)
            ? timers.Timers[slotIndex]
            : null;

    private static float Brightness(int active)
    {
        var darkness = FullyLit - Math.Min(FullyLit, active);
        return darkness > 0 ? Gamma / (darkness * darkness + Gamma) : 1f;
    }
}

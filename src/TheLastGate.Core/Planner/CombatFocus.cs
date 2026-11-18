using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

public sealed class CombatFocusInput
{
    // The planner's state, plan included.
    public BuffModelInput Model { get; set; } = new();

    // The weapon, off-hand, race and server values; the shown skills and base Weapon and Armor Value are worked out
    // again for each mix.
    public CombatModelInput Combat { get; set; } = new();

    public BuffModelSkill[] CombatSkills { get; } = BuffFocusInput.NewCombatSkills();

    // The stat raise slot of each combat skill; NoSlot when not known.
    public int[] CombatRaiseIndex { get; } = NoSlots(CombatModel.SkillCount);

    public int[] SpellRaiseIndex { get; } = NoSlots(BuffModelInput.BuffCount);

    public CombatStat Stat { get; set; }

    public List<FocusCandidate> Candidates { get; } = [];

    private static int[] NoSlots(int count)
    {
        var slots = new int[count];
        Array.Fill(slots, BuffFocus.NoSlot);
        return slots;
    }
}

// The cheapest route to +1 on the stat; a tie goes to fewer points.
public sealed record CombatFocusResult(int CurrentValue, FocusRoute? Best);

public static class CombatFocus
{
    public static CombatFocusResult Search(CombatFocusInput input) => throw new NotImplementedException();
}

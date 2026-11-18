using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

public sealed class BuffFocusInput
{
    // The planner's state, plan included.
    public BuffModelInput Model { get; set; } = new();

    public BuffModelSkill Focused { get; set; } = new();

    public int FocusedRaiseIndex { get; set; }

    // The stat raise slot of each spell skill; NoSlot when not known.
    public int[] SpellRaiseIndex { get; } = [BuffFocus.NoSlot, BuffFocus.NoSlot, BuffFocus.NoSlot];

    // The focused Resistance or Immunity gains a tenth of the shown AV.
    public bool ArmorFeedsFocused { get; set; }

    // Read only when armor feeds the focused skill.
    public CombatModelInput Combat { get; set; } = new();

    public BuffModelSkill[] CombatSkills { get; } = NewCombatSkills();

    public int[] CombatRaiseIndex { get; } = new int[CombatModel.SkillCount];

    public List<FocusCandidate> Candidates { get; } = [];

    internal static BuffModelSkill[] NewCombatSkills()
    {
        var skills = new BuffModelSkill[CombatModel.SkillCount];
        for (var skill = 0; skill < skills.Length; skill++)
        {
            skills[skill] = new BuffModelSkill();
        }

        return skills;
    }
}

public sealed record BuffFocusResult(
    int CurrentValue,
    FocusRoute? Best,
    FocusRoute? Direct,
    FocusRoute? ThroughAttributes,
    FocusRoute? ThroughBless);

public static class BuffFocus
{
    public const int NoSlot = -1;
    public const int AttributePool = -2;
    public const int HitpointsRaise = 5;
    public const int ManaRaise = 7;

    public static BuffFocusResult Search(BuffFocusInput input) => throw new NotImplementedException();
}

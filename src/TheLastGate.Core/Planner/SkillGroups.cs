using System;

namespace TheLastGate.Planner;

[Flags]
public enum SkillGroups
{
    None = 0,
    Passive = 1,
    ActiveSpell = 2,
    ActiveMelee = 4,
}

public static class SkillGroupTable
{
    public static SkillGroups Of(int skillNumber) => throw new NotImplementedException();
}

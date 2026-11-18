using System;

namespace TheLastGate.Planner;

public static class FocusInputs
{
    public static SkillFocusInput Plain(PlannerSheet sheet, int skill) => throw new NotImplementedException();

    public static BuffFocusInput? Buffed(PlannerSheet sheet, int skill) => throw new NotImplementedException();

    public static CombatFocusInput? Combat(PlannerSheet sheet, CombatStat stat) =>
        throw new NotImplementedException();
}

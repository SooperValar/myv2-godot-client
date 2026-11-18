using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

// One row of the planner: an attribute, Hitpoints, Mana or a skill, by its stat plan slot.
public sealed record PlannerRow(
    string Name,
    int Slot,
    int Base,
    int Current,
    int DirectRaise,
    int AttributeGain,
    int BuffShift,
    int? Cost,
    bool CanRaise,
    bool CanLower,
    bool UsesRaisedAttribute)
{
    // What the row shows with the plan: the server's value, the buffs toggled, the points bought and what the raised
    // attributes add.
    public int Preview => Current + BuffShift + DirectRaise + AttributeGain;
}

// The planner's numbers for one look at the game: the buff model with and without the plan, the combat model's input
// and every row. Without the spell meta stats the rows fall back to the plain skill preview.
public sealed class PlannerSheet
{
    public bool BuffsAvailable => throw new NotImplementedException();

    public BuffModelInput? PlannedInput => throw new NotImplementedException();

    public BuffModelResult? Planned => throw new NotImplementedException();

    public CombatModelInput? Combat => throw new NotImplementedException();

    public bool ChoicesChanged => throw new NotImplementedException();

    public IReadOnlyList<PlannerRow> Attributes => throw new NotImplementedException();

    public IReadOnlyList<PlannerRow> Vitals => throw new NotImplementedException();

    public IReadOnlyList<PlannerRow> Skills => throw new NotImplementedException();

    public static PlannerSheet For(PlannerGame game) => throw new NotImplementedException();

    public static int SpellRaceModifier(uint kindred) => throw new NotImplementedException();

    public static int EmptyGearSlots(IReadOnlyList<int> wornSprites) => throw new NotImplementedException();
}

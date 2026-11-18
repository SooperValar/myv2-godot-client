using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

public enum FocusTone
{
    Heading,
    Normal,
    SkillRaise,
    AttributeRaise,
    OtherRoute,
}

public sealed record FocusLine(string Text, string? Value, bool Indented, FocusTone Tone);

public sealed class FocusPanel
{
    public const string Prompt = "Click a skill or combat stat to focus it";

    public IReadOnlyList<FocusLine> Lines { get; private init; } = [];

    public IReadOnlyList<FocusRaise> Plan { get; private init; } = [];

    public int PlanCost { get; private init; }

    public bool Affordable { get; private init; }

    public bool LostFocus { get; private init; }

    public static FocusPanel For(PlannerSheet sheet, PlannerFocus? focus, FocusCache cache) =>
        throw new NotImplementedException();
}

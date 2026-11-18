using System;
using System.Collections.Generic;
using TheLastGate.Buffs;

namespace TheLastGate.Planner;

public sealed record AdvancedSpell(BuffKind Kind, string Label, bool Ready, string? Warning);

public sealed class AdvancedSheet
{
    public IReadOnlyList<AdvancedSpell> Spells { get; private init; } = [];

    public IReadOnlyList<string> TreeRows { get; private init; } = [];

    public IReadOnlyList<int> TreeCopies { get; private init; } = [];

    public static AdvancedSheet For(PlannerSheet sheet) => throw new NotImplementedException();

    public static string CardLabel(TarotCard card) => throw new NotImplementedException();

    public static string CorruptorEffect(int corruptor) => throw new NotImplementedException();

    public static string GearEffect(int gear) => throw new NotImplementedException();

    public static string TreeCopiesText(int copies) => throw new NotImplementedException();
}

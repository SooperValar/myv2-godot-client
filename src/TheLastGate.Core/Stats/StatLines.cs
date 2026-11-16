using System.Collections.Generic;
using TheLastGate.Assets;

namespace TheLastGate.Stats;

public static class StatLines
{
    private const int BaseValue = 0;
    private const int ScoreHigh = 4;
    private const int ScoreLow = 5;
    private const int Maximum = 5;
    private const int KnownSkill = 1;
    private const int NotLearntSkill = 2;
    private const int RedSkill = 4;
    private const int NotLearntValue = 300;

    public static readonly IReadOnlyList<string> AttributeNames =
        ["Braveness", "Willpower", "Intuition", "Agility", "Strength"];

    public static StatLine Attribute(int attribute, int[] values, StatPlan plan, int points) =>
        Raisable(AttributeNames[attribute], attribute, values, Score(values), plan, points);

    public static StatLine HitPoints(int[] values, StatPlan plan, int points) =>
        Raisable("Hitpoints", StatPlan.HitPointsSlot, values, values[Maximum], plan, points);

    public static StatLine Mana(int[] values, StatPlan plan, int points) =>
        Raisable("Mana", StatPlan.ManaSlot, values, values[Maximum], plan, points);

    public static StatLine Skill(int number, string name, int show, int[] values, StatPlan plan, int points) =>
        show switch
        {
            < KnownSkill => StatLine.Unknown,
            RedSkill => new StatLine(name, TextColour.Red, null, Score(values), false, null),
            NotLearntSkill => new StatLine(name, TextColour.Orange, null, NotLearntValue, false, null),
            _ => Raisable(name, StatPlan.SkillSlot(number), values, Score(values), plan, points),
        };

    private static StatLine Raisable(string name, int slot, int[] values, int current, StatPlan plan, int points)
    {
        var planned = plan.Raised(slot);
        var value = values[BaseValue] + planned;
        var cost = plan.NextCost(slot, values);
        return new StatLine(
            name,
            TextColour.Yellow,
            value > 0 ? value : null,
            current + planned,
            cost <= points - plan.PointsUsed,
            cost,
            planned > 0);
    }

    private static int Score(int[] values) => (values[ScoreHigh] << 8) | values[ScoreLow];
}

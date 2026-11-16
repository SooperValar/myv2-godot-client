using System;
using TheLastGate.Assets;

namespace TheLastGate.Stats;

public static class StatLines
{
    private const int BaseValue = 0;
    private const int Limit = 2;
    private const int Difficulty = 3;
    private const int ScoreHigh = 4;
    private const int ScoreLow = 5;
    private const int Maximum = 5;
    private const int KnownSkill = 1;
    private const int NotLearntSkill = 2;
    private const int RedSkill = 4;
    private const int NotLearntValue = 300;

    private static readonly string[] AttributeNames = ["Braveness", "Willpower", "Intuition", "Agility", "Strength"];

    public static StatLine Attribute(int attribute, int[] values, StatPlan plan, int points)
    {
        var value = values[BaseValue];
        var cost = value >= values[Limit] ? (int?)null : value * value * value * values[Difficulty] / 20;
        return Raisable(AttributeNames[attribute], value, Score(values), cost, points);
    }

    public static StatLine HitPoints(int[] values, StatPlan plan, int points) => Pool("Hitpoints", values, points);

    public static StatLine Mana(int[] values, StatPlan plan, int points) => Pool("Mana", values, points);

    public static StatLine Skill(int number, string name, int show, int[] values, StatPlan plan, int points)
    {
        switch (show)
        {
            case < KnownSkill:
                return StatLine.Unknown;
            case RedSkill:
                return new StatLine(name, TextColour.Red, null, Score(values), false, null);
            case NotLearntSkill:
                return new StatLine(name, TextColour.Orange, null, NotLearntValue, false, null);
        }

        var value = values[BaseValue];
        var cost = value >= values[Limit]
            ? (int?)null
            : Math.Max(value, value * value * value * values[Difficulty] / 40);
        return Raisable(name, value, Score(values), cost, points);
    }

    private static StatLine Pool(string name, int[] values, int points)
    {
        var value = values[BaseValue];
        var cost = value >= values[Limit] ? (int?)null : value * values[Difficulty];
        return Raisable(name, value, values[Maximum], cost, points);
    }

    private static StatLine Raisable(string name, int value, int current, int? cost, int points) =>
        new(
            name,
            TextColour.Yellow,
            value > 0 ? value : null,
            current,
            cost <= points,
            value > 0 ? cost : null);

    private static int Score(int[] values) => (values[ScoreHigh] << 8) | values[ScoreLow];
}

using System;
using System.Collections.Generic;
using TheLastGate.Buffs;
using TheLastGate.State;
using TheLastGate.Stats;

namespace TheLastGate.Planner;

// A skill as the server's skill table describes it.
public sealed record PlannerSkillText(string Name, IReadOnlyList<int> Attributes, int Show);

// What the planner works from: the player's numbers copied out of the game, and the plan and choices it changes.
public sealed class PlannerGame
{
    public const int StatValueCount = PlayerState.StatValueCount;

    public int Points { get; set; }

    public uint Kindred { get; set; }

    public bool HardMode { get; set; }

    public int Weapon { get; set; }

    public int Armor { get; set; }

    public int TreePoints { get; set; }

    public byte[] TreeNodes { get; } = new byte[PlayerState.TreeNodeCount];

    public int[] WornSprites { get; } = new int[PlayerState.WornSlots];

    public int[][] Attributes { get; } = Values(PlayerState.AttributeCount);

    public int[][] Skills { get; } = Values(PlayerState.SkillSlots);

    public int[] HitPoints { get; } = new int[StatValueCount];

    public int[] Mana { get; } = new int[StatValueCount];

    public PlannerSkillText[] SkillTexts { get; set; } = [];

    // The skill numbers in the order the skill list shows them.
    public IReadOnlyList<int> SkillOrder { get; set; } = [];

    // The meta stats that have arrived, by name.
    public Dictionary<string, int> MetaStats { get; } = [];

    public bool[] BuffActive { get; } = new bool[BuffModelInput.BuffCount];

    public BuffReading?[] BuffReadings { get; } = new BuffReading?[BuffModelInput.BuffCount];

    public StatPlan Plan { get; set; } = new();

    public PlannerChoices Choices { get; set; } = new();

    public static PlannerGame From(
        PlayerState player,
        Terminology terminology,
        StatPlan plan,
        PlannerChoices choices,
        BuffWatch buffs,
        bool hardMode) =>
        throw new NotImplementedException();

    public int[] ValuesFor(int slot) => throw new NotImplementedException();

    public static int Score(int[] values) => throw new NotImplementedException();

    private static int[][] Values(int count)
    {
        var values = new int[count][];
        for (var index = 0; index < count; index++)
        {
            values[index] = new int[StatValueCount];
        }

        return values;
    }
}

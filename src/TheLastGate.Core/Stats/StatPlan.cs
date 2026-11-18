using System;
using System.Collections.Generic;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Stats;

public sealed class StatPlan
{
    public const int HitPointsSlot = 5;
    public const int ManaSlot = 7;
    public const int FirstSkillSlot = 8;
    public const string UpdateHelp = "Make the changes permanent";

    private const int SlotCount = FirstSkillSlot + PlayerState.SkillSlots;
    private const int BaseValue = 0;
    private const int Limit = 2;
    private const int Difficulty = 3;
    private const int MostSkillPointsPerCommand = 135;
    private const int MostPointsPerCommand = 900;
    private const int ShiftClickTimes = 10;
    private const int CtrlClickTimes = 90;

    private readonly int[] raised = new int[SlotCount];

    public int PointsUsed { get; private set; }

    public static int SkillSlot(int skillNumber) => FirstSkillSlot + skillNumber;

    public static int[] Values(PlayerState player, int slot) => slot switch
    {
        < PlayerState.AttributeCount => player.Attributes[slot],
        HitPointsSlot => player.HitPoints,
        ManaSlot => player.Mana,
        _ => player.Skills[slot - FirstSkillSlot],
    };

    public static int ClickTimes(bool shift, bool ctrlOrAlt) =>
        ctrlOrAlt ? CtrlClickTimes : shift ? ShiftClickTimes : 1;

    public static string RaiseHelp(string name) => $"Raise {name}.";

    public static string LowerHelp(string name) => $"Lower {name}.";

    public int Raised(int slot) => raised[slot];

    public int? NextCost(int slot, int[] values) => CostAt(slot, values, raised[slot]);

    public void Raise(int slot, int[] values, int points, int times)
    {
        for (var time = 0; time < times; time++)
        {
            var cost = NextCost(slot, values);
            if (cost == null || cost > points - PointsUsed)
            {
                return;
            }

            PointsUsed += cost.Value;
            raised[slot]++;
        }
    }

    public void Lower(int slot, int[] values, int times)
    {
        for (var time = 0; time < times && raised[slot] > 0; time++)
        {
            raised[slot]--;
            PointsUsed -= CostAt(slot, values, raised[slot]) ?? 0;
        }
    }

    public void Reset() => throw new NotImplementedException();

    public IReadOnlyList<byte[]> Commit()
    {
        var packets = new List<byte[]>();
        for (var slot = 0; slot < SlotCount; slot++)
        {
            var mostPerCommand = slot >= FirstSkillSlot ? MostSkillPointsPerCommand : MostPointsPerCommand;
            for (var left = raised[slot]; left > 0; left -= mostPerCommand)
            {
                packets.Add(ClientPackets.ShortAndInt(ClientCommand.Stat, slot, Math.Min(left, mostPerCommand)));
            }
        }

        Array.Clear(raised);
        PointsUsed = 0;
        return packets;
    }

    private static int? CostAt(int slot, int[] values, int planned)
    {
        var value = values[BaseValue] + planned;
        if (values[BaseValue] <= 0 || value >= values[Limit])
        {
            return null;
        }

        var difficulty = values[Difficulty];
        return slot switch
        {
            < PlayerState.AttributeCount => value * value * value * difficulty / 20,
            HitPointsSlot or ManaSlot => value * difficulty,
            _ => Math.Max(value, value * value * value * difficulty / 40),
        };
    }
}

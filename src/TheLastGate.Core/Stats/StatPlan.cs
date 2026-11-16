using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Stats;

public sealed class StatPlan
{
    public const int HitPointsSlot = 5;
    public const int ManaSlot = 7;
    public const int FirstSkillSlot = 8;
    public const string UpdateHelp = "Make the changes permanent";

    public int PointsUsed { get; private set; }

    public static int SkillSlot(int skillNumber) => throw new NotImplementedException();

    public static int[] Values(PlayerState player, int slot) => throw new NotImplementedException();

    public static int ClickTimes(bool shift, bool ctrlOrAlt) => throw new NotImplementedException();

    public static string RaiseHelp(string name) => throw new NotImplementedException();

    public static string LowerHelp(string name) => throw new NotImplementedException();

    public int Raised(int slot) => throw new NotImplementedException();

    public int? NextCost(int slot, int[] values) => throw new NotImplementedException();

    public void Raise(int slot, int[] values, int points, int times) => throw new NotImplementedException();

    public void Lower(int slot, int[] values, int times) => throw new NotImplementedException();

    public IReadOnlyList<byte[]> Commit() => throw new NotImplementedException();
}

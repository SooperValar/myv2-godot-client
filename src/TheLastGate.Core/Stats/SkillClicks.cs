using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Stats;

public static class SkillClicks
{
    public static byte[] Use(int skillNumber, int selectedCharacter, int firstAttribute) =>
        throw new NotImplementedException();

    public static bool IsKnown(int skillNumber, bool learnt, uint kindred) => throw new NotImplementedException();

    public static IReadOnlyList<LogLine> Help(string name, string description, int[] attributes) =>
        throw new NotImplementedException();

    public static LogLine StatHelp(int slot) => throw new NotImplementedException();
}

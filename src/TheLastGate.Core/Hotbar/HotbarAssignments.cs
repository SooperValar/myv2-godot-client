using System;
using System.Collections.Generic;
using TheLastGate.Characters;
using TheLastGate.State;

namespace TheLastGate.Hotbar;

public static class HotbarAssignments
{
    public static LogLine Assign(CharacterSettings settings, int slot, int skillNumber, string skillName) =>
        throw new NotImplementedException();

    public static void Clear(CharacterSettings settings, int slot) => throw new NotImplementedException();

    public static IReadOnlyList<int> MenuSkills(int[][] skills, uint kindred) => throw new NotImplementedException();
}

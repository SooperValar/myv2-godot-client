using System.Collections.Generic;
using System.Linq;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.State;

namespace TheLastGate.Hotbar;

public static class HotbarAssignments
{
    private const int ButtonNameLength = 7;
    private const int Identify = 52;

    // The skills the MyV2 server does something with when used (skill_driver.c), buffs first.
    private static readonly int[] UsableSkills = [21, 18, 17, 11, 26, 24, 20, 19, 25, 27, 35, 16, 22, 14, 15, 13];

    public static LogLine Assign(
        CharacterSettings settings,
        int slot,
        int skillNumber,
        string skillName,
        string keyName)
    {
        if (settings.HotbarActions[slot] == skillNumber)
        {
            Clear(settings, slot);
            return Line($"{keyName} is now unassigned.");
        }

        settings.HotbarActions[slot] = skillNumber;
        settings.HotbarNames[slot] = skillName.Length > ButtonNameLength ? skillName[..ButtonNameLength] : skillName;
        return Line($"{keyName} is now {skillName}.");
    }

    public static void Clear(CharacterSettings settings, int slot)
    {
        settings.HotbarActions[slot] = CharacterSettings.Unassigned;
        settings.HotbarNames[slot] = CharacterSettings.EmptyHotbarName;
    }

    public static IReadOnlyList<int> MenuSkills(int[][] skills) =>
        UsableSkills.Where(skill => skills[skill][0] != 0).ToList();

    private static LogLine Line(string text) => new(text, (int)TextColour.Yellow);
}

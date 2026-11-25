using System;
using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Stats;

// The order merc.exe lists skills in: learnt ones first, ones fixed at their limit below them, then by group and name.
public static class SkillOrder
{
    public static int[] Sorted(IEnumerable<SkillEntry> skills)
    {
        var sorted = skills.ToArray();
        Array.Sort(sorted, Compare);
        return Array.ConvertAll(sorted, skill => skill.Number);
    }

    private static int Compare(SkillEntry first, SkillEntry second)
    {
        if (first.Number == second.Number)
        {
            return 0;
        }

        if (Last(first, second, skill => skill.Name.Length == 0) is { } unnamed)
        {
            return unnamed;
        }

        if (first.Learnt != second.Learnt)
        {
            return first.Learnt ? -1 : 1;
        }

        if (Last(first, second, skill => skill.Fixed) is { } fixedSkill)
        {
            return fixedSkill;
        }

        return first.SortKey != second.SortKey
            ? first.SortKey.CompareTo(second.SortKey)
            : string.CompareOrdinal(first.Name, second.Name);
    }

    private static int? Last(SkillEntry first, SkillEntry second, Func<SkillEntry, bool> goesLast)
    {
        var firstLast = goesLast(first);
        return firstLast == goesLast(second) ? null : firstLast ? 1 : -1;
    }
}

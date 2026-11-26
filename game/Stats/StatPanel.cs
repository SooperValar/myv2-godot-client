using System;
using System.Diagnostics;
using System.Linq;
using TheLastGate.State;

namespace TheLastGate.Stats;

public sealed class StatPanel
{
    public const int TopRows = 7;
    // Past the end of a short list: a slot the server never shows, so the row stays blank.
    public const int BlankMeta = Terminology.MetaStatCount - 1;

    private int[] offenseRows = MyV2MetaStats.OffenseSlots;
    private int[] defenseRows = MyV2MetaStats.DefenseSlots;

    public StatListMode Mode { get; private set; }

    public SkillScroll Skills { get; } = new();

    public int MetaRow { get; private set; }

    public bool ShowsMetaStats => Mode is StatListMode.Offenses or StatListMode.Defenses;

    public bool ShowsMetaOnTop => Mode != StatListMode.Skills;

    public int Row => ShowsMetaStats ? MetaRow : Skills.Row;

    public int LastRow => ShowsMetaStats ? LastMetaRow : SkillScroll.LastRow;

    public int LastMetaRow => Math.Max(0, MetaRows.Length - SkillScroll.VisibleRows);

    private int[] MetaRows => Mode == StatListMode.Defenses ? defenseRows : offenseRows;

    public static string Help(StatListMode button) => button switch
    {
        StatListMode.Skills or StatListMode.SkillsAndMeta =>
            "Makes the left panel display skill scores and experience points (default).",
        StatListMode.Offenses =>
            "Makes the left panel display Offense-related statistics, such as attack damage and critical hit rate.",
        StatListMode.Defenses =>
            "Makes the left panel display Defense-related statistics, such as eHP and regeneration rates.",
        _ => throw new UnreachableException(),
    };

    public void Choose(StatListMode button) =>
        Mode = button == StatListMode.Skills && Mode == StatListMode.Skills ? StatListMode.SkillsAndMeta : button;

    public bool IsMarked(StatListMode button) =>
        button == (Mode == StatListMode.SkillsAndMeta ? StatListMode.Skills : Mode);

    public bool ListShown(Func<int, bool> isShown)
    {
        var offenses = Shown(MyV2MetaStats.OffenseSlots, isShown);
        var defenses = Shown(MyV2MetaStats.DefenseSlots, isShown);
        var changed = !offenses.SequenceEqual(offenseRows) || !defenses.SequenceEqual(defenseRows);
        offenseRows = offenses;
        defenseRows = defenses;
        MetaRow = Math.Clamp(MetaRow, 0, LastMetaRow);
        return changed;
    }

    public void ScrollBy(int rows) => ScrollTo(Row + rows);

    public void ScrollTo(int row)
    {
        if (ShowsMetaStats)
        {
            MetaRow = Math.Clamp(row, 0, LastMetaRow);
        }
        else
        {
            Skills.ScrollTo(row);
        }
    }

    public int? TopMeta(int row) => ShowsMetaOnTop ? row : null;

    public int? BottomMeta(int row) => Mode switch
    {
        StatListMode.Skills or StatListMode.SkillsAndMeta => null,
        StatListMode.Offenses or StatListMode.Defenses =>
            row + MetaRow < MetaRows.Length ? MetaRows[row + MetaRow] : BlankMeta,
        _ => throw new UnreachableException(),
    };

    private static int[] Shown(int[] slots, Func<int, bool> isShown) => slots.Where(isShown).ToArray();
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.State;

// MyV2's Help window pages (the server's help.c): each comes as SV_HELP pieces, and a text longer than one packet
// comes in parts that are joined.
public enum HelpPieceKind
{
    Start = 0,
    Heading = 1,
    Cell = 2,
    Line = 3,
    End = 255,
}

public enum HelpPageNumber
{
    Commands = 1,
    Quests = 2,
    Quest = 3,
    Areas = 4,
    KillQuests = 5,
    Ranks = 6,
    Who = 7,
    Armor = 8,
    Weapons = 9,
    Rings = 10,
    Belts = 11,
    SpecialItems = 12,
}

public enum WeaponList
{
    Daggers = 0,
    Swords = 1,
    TwoHanded = 2,
    Axes = 3,
    Greataxes = 4,
    Staffs = 5,
}

public sealed record HelpHeading(string Text, bool RightAligned);

public sealed record HelpRow(int Font, IReadOnlyList<string> Cells);

public sealed record HelpLine(int Font, string Text);

public sealed record HelpPage(
    int Argument,
    IReadOnlyList<HelpHeading> Headings,
    IReadOnlyList<HelpRow> Rows,
    IReadOnlyList<HelpLine> Lines);

public sealed class HelpPages
{
    public const int AllQuests = 1;
    public const int QuestOfAll = 0x8000;

    private readonly Dictionary<int, HelpPage> pages = [];
    private readonly List<(string Text, bool Right)> headings = [];
    private readonly List<(int Font, List<string> Cells)> rows = [];
    private readonly List<(int Font, string Text)> lines = [];
    private int incomingArgument;

    public event Action<HelpPageNumber>? PageArrived;

    public HelpPage? Page(HelpPageNumber number) => pages.GetValueOrDefault((int)number);

    internal void Receive(int page, HelpPieceKind kind, int row, int column, int font, int part, string text)
    {
        switch (kind)
        {
            case HelpPieceKind.Start:
                incomingArgument = row;
                headings.Clear();
                rows.Clear();
                lines.Clear();
                break;
            case HelpPieceKind.Heading:
                while (headings.Count <= column)
                {
                    headings.Add(("", false));
                }

                headings[column] = (Joined(headings[column].Text, part, text), font != 0);
                break;
            case HelpPieceKind.Cell:
                while (rows.Count <= row)
                {
                    rows.Add((font, []));
                }

                var cells = rows[row].Cells;
                while (cells.Count <= column)
                {
                    cells.Add("");
                }

                cells[column] = Joined(cells[column], part, text);
                rows[row] = (font, cells);
                break;
            case HelpPieceKind.Line:
                while (lines.Count <= row)
                {
                    lines.Add((font, ""));
                }

                lines[row] = (font, Joined(lines[row].Text, part, text));
                break;
            case HelpPieceKind.End:
                pages[page] = new HelpPage(
                    incomingArgument,
                    headings.Select(heading => new HelpHeading(heading.Text, heading.Right)).ToArray(),
                    rows.Select(shown => new HelpRow(shown.Font, shown.Cells.ToArray())).ToArray(),
                    lines.Select(line => new HelpLine(line.Font, line.Text)).ToArray());
                PageArrived?.Invoke((HelpPageNumber)page);
                break;
        }
    }

    private static string Joined(string before, int part, string text) => part == 0 ? text : before + text;
}

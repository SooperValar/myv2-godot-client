using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Windows;

// The current client's message buffer: it is never cleared, its line cursor runs on to 58 before starting at the top
// again, and only the first 27 lines are ever shown. A long line wraps at the last space within 42 characters; a word
// too long to wrap stays whole and its end is repeated on the next line.
public sealed class MessageOfTheDayLines
{
    public const int VisibleLines = 27;

    private const int WrapColumn = 42;
    private const int MaxLineLength = 59;
    private const int CursorLimit = 59;

    private readonly LogLine[] lines = CreateLines();
    private int cursor;

    public IReadOnlyList<LogLine> Lines => lines;

    public void Add(LogLine line)
    {
        var text = line.Text;
        if (text.Length == 0)
        {
            return;
        }

        for (var written = 0; written < VisibleLines; written++)
        {
            if (cursor >= CursorLimit)
            {
                cursor = 0;
            }

            var stored = text.Length > MaxLineLength ? text[..MaxLineLength] : text;
            if (text.Length < WrapColumn)
            {
                Store(stored, line.Font);
                break;
            }

            var space = stored.LastIndexOf(' ', Math.Min(WrapColumn, stored.Length - 1));
            if (space > 0)
            {
                Store(stored[..space], line.Font);
                text = text[(space + 1)..];
            }
            else
            {
                Store(stored, line.Font);
                text = text[WrapColumn..];
            }

            cursor++;
        }

        cursor++;
    }

    private static LogLine[] CreateLines()
    {
        var created = new LogLine[VisibleLines];
        Array.Fill(created, new LogLine("", 0));
        return created;
    }

    private void Store(string text, int font)
    {
        if (cursor < VisibleLines)
        {
            lines[cursor] = new LogLine(text, font);
        }
    }
}

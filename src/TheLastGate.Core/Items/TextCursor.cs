using System;
using System.Globalization;

namespace TheLastGate.Items;

// Reads numbers the way the C library's strtol and strtof do: leading white space and a sign are allowed, and
// nothing moves when there is no number.
internal sealed class TextCursor(string text)
{
    private int position;

    public bool AtEnd => position == text.Length;

    public bool IsBlank()
    {
        var end = position;
        while (end < text.Length && text[end] == ' ')
        {
            end++;
        }

        return end == text.Length;
    }

    public bool Expect(string literal)
    {
        var start = SkipSpaces(text, position);
        var wanted = literal.TrimStart(' ');
        if (start + wanted.Length > text.Length || string.CompareOrdinal(text, start, wanted, 0, wanted.Length) != 0)
        {
            return false;
        }

        position = start + wanted.Length;
        return true;
    }

    public bool Skip(char character)
    {
        if (position < text.Length && text[position] == character)
        {
            position++;
            return true;
        }

        return false;
    }

    public bool ReadInt(out int value)
    {
        value = 0;
        var start = SkipWhiteSpace(position);
        var end = SignedDigits(out var digitsStart);
        var number = text.AsSpan(start, end - start);
        if (end == digitsStart
            || !long.TryParse(number, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed)
            || parsed is < int.MinValue or > int.MaxValue)
        {
            return false;
        }

        value = (int)parsed;
        position = end;
        return true;
    }

    // "+1.5%" is 15 tenths of a percent.
    public bool ReadPercent(out int tenths)
    {
        tenths = 0;
        var start = SkipWhiteSpace(position);
        var end = SignedDigits(out var digitsStart);
        var fractionEnd = end;
        if (fractionEnd < text.Length && text[fractionEnd] == '.')
        {
            fractionEnd++;
            while (fractionEnd < text.Length && char.IsAsciiDigit(text[fractionEnd]))
            {
                fractionEnd++;
            }
        }

        var hasDigits = end > digitsStart || fractionEnd > end + 1;
        var number = text.AsSpan(start, fractionEnd - start);
        if (!hasDigits || fractionEnd >= text.Length || text[fractionEnd] != '%'
            || !float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return false;
        }

        tenths = (int)MathF.Round(parsed * 10f, MidpointRounding.AwayFromZero);
        position = fractionEnd + 1;
        return true;
    }

    public bool ReadUntil(char close, int maxLength, out string read)
    {
        read = "";
        var end = text.IndexOf(close, position);
        if (end < 0 || end == position || end - position >= maxLength)
        {
            return false;
        }

        read = text[position..end];
        position = end + 1;
        return true;
    }

    private static int SkipSpaces(string text, int index)
    {
        while (index < text.Length && text[index] == ' ')
        {
            index++;
        }

        return index;
    }

    private int SkipWhiteSpace(int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    private int SignedDigits(out int digitsStart)
    {
        var index = SkipWhiteSpace(position);
        if (index < text.Length && text[index] is '+' or '-')
        {
            index++;
        }

        digitsStart = index;
        while (index < text.Length && char.IsAsciiDigit(text[index]))
        {
            index++;
        }

        return index;
    }
}

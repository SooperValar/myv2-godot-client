using System;
using System.Text;

namespace TheLastGate.Characters;

// Text in the C client's fixed char[] fields: Latin-1, ended by the first NUL.
public static class NulTerminatedText
{
    public static string Read(ReadOnlySpan<byte> field)
    {
        var end = field.IndexOf((byte)0);
        return Encoding.Latin1.GetString(end < 0 ? field : field[..end]);
    }

    public static void Write(Span<byte> field, string text)
    {
        field.Clear();
        var textBytes = Encoding.Latin1.GetBytes(text);
        textBytes.AsSpan(0, Math.Min(textBytes.Length, field.Length - 1)).CopyTo(field);
    }
}

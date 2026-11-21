namespace TheLastGate.Notes;

internal static class NoteText
{
    // Cut to a number of UTF-8 bytes, always between characters.
    public static string Fit(string text, int maxBytes)
    {
        var length = 0;
        var bytes = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            bytes += rune.Utf8SequenceLength;
            if (bytes > maxBytes)
            {
                break;
            }

            length += rune.Utf16SequenceLength;
        }

        return text[..length];
    }

    // The current client folds only the ASCII letters when comparing names.
    public static bool SameName(string left, string right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var index = 0; index < left.Length; index++)
        {
            var leftCharacter = left[index];
            var rightCharacter = right[index];
            if (leftCharacter != rightCharacter
                && !(char.IsAsciiLetter(leftCharacter) && char.IsAsciiLetter(rightCharacter)
                    && char.ToLowerInvariant(leftCharacter) == char.ToLowerInvariant(rightCharacter)))
            {
                return false;
            }
        }

        return true;
    }
}

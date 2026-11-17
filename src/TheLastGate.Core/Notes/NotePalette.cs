namespace TheLastGate.Notes;

public static class NotePalette
{
    public const int Size = 8;

    private static readonly uint[] Colours =
        [0x4FC3FF, 0xFFD34D, 0xFF6B6B, 0x7CFC8A, 0xD58CFF, 0xFFA040, 0xFFFFFF, 0x40E0D0];

    public static uint Rgb(int colourIndex) => colourIndex is >= 0 and < Size ? Colours[colourIndex] : Colours[0];
}

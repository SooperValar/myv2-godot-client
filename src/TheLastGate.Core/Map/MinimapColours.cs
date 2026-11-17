namespace TheLastGate.Map;

// Colours are 0x00RRGGBB, and 0 means a tile nobody has seen.
public static class MinimapColours
{
    public static uint FromRgb(byte red, byte green, byte blue) => ((uint)red << 16) | ((uint)green << 8) | blue;

    public static uint FromRgb555(ushort packed) =>
        FromRgb(SpreadFiveBits(packed >> 10), SpreadFiveBits(packed >> 5), SpreadFiveBits(packed));

    private static byte SpreadFiveBits(int bits)
    {
        var shifted = (bits & 0x1F) << 3;
        return (byte)(shifted | (shifted >> 5));
    }
}

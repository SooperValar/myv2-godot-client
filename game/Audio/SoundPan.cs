using System;

namespace TheLastGate.Audio;

// The server's pan is in hundredths of a decibel and points the other way: the far side is turned down by that much.
public static class SoundPan
{
    private const float HundredthsPerDecibel = 100f;
    private const float DecibelsPerTenfold = 20f;

    public static (float Left, float Right) Gains(int serverPan)
    {
        var farSide = MathF.Pow(10f, -Math.Abs(serverPan) / HundredthsPerDecibel / DecibelsPerTenfold);
        return serverPan switch
        {
            > 0 => (1f, farSide),
            < 0 => (farSide, 1f),
            _ => (1f, 1f),
        };
    }

    // Godot's panner scales the left side by 1 - pan and the right by 1 + pan, so the louder side is boosted too;
    // the volume scale brings it back to full.
    public static (float Pan, float VolumeScale) Panner(int serverPan)
    {
        var (left, right) = Gains(serverPan);
        var farSide = Math.Min(left, right);
        var pan = (1f - farSide) / (1f + farSide);
        return (right < left ? -pan : pan, (1f + farSide) / 2f);
    }
}

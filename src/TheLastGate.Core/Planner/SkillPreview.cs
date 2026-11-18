using System;

namespace TheLastGate.Planner;

public static class SkillPreview
{
    public const int NormalDivisor = 5;
    public const int HardModeDivisor = 6;

    public static int AttributeDivisor(bool hardMode) => throw new NotImplementedException();

    public static int AttributeGain(ReadOnlySpan<int> values, ReadOnlySpan<int> raises, int divisor) =>
        throw new NotImplementedException();

    public static bool UsesRaisedAttribute(ReadOnlySpan<int> raises) => throw new NotImplementedException();
}

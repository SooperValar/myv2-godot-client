using System;

namespace TheLastGate.Map;

public readonly record struct LabelRect(float Left, float Top, float Right, float Bottom)
{
    public bool Overlaps(LabelRect other) => throw new NotImplementedException();
}

public sealed class LabelLayout
{
    public const int DefaultCapacity = 4096;

    public LabelLayout(int capacity = DefaultCapacity) => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool Reserve(LabelRect rect) => throw new NotImplementedException();

    public LabelRect? PlaceBeside(float anchorX, float anchorY, float clearance, float width, float height) =>
        throw new NotImplementedException();
}

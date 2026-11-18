using System;
using System.Linq;

namespace TheLastGate.Planner;

// The server's order for the five attributes.
public enum AttributeSlot
{
    Braveness,
    Willpower,
    Intuition,
    Agility,
    Strength,
}

// An amount for each attribute, equal to another with the same amounts.
public sealed class AttributeAmounts : IEquatable<AttributeAmounts>
{
    public const int Count = 5;

    private readonly int[] amounts = new int[Count];

    public int this[AttributeSlot attribute]
    {
        get => amounts[(int)attribute];
        set => amounts[(int)attribute] = value;
    }

    public bool Equals(AttributeAmounts? other) => other != null && amounts.SequenceEqual(other.amounts);

    public override bool Equals(object? other) => Equals(other as AttributeAmounts);

    public override int GetHashCode() =>
        HashCode.Combine(amounts[0], amounts[1], amounts[2], amounts[3], amounts[4]);
}

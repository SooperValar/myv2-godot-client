using TheLastGate.Planner;

namespace TheLastGate.Buffs;

// The three buffs the server's buff list is read for.
public enum BuffKind
{
    Bless,
    Enhance,
    Protect,
}

// One buff as the server's buff list describes it: its power, and with spell knowledge the exact bonuses it gives.
public sealed class BuffReading
{
    public bool Known { get; set; }

    public int Power { get; set; }

    public bool HasExact { get; set; }

    public AttributeAmounts AttributeBonus { get; } = new();

    public int WeaponBonus { get; set; }

    public int ArmorBonus { get; set; }
}

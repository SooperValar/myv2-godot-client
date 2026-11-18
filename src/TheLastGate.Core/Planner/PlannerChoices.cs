using System;

namespace TheLastGate.Planner;

// The planner's picks for one character. A weapon or off-hand not picked yet follows the character's best skill.
public sealed class PlannerChoices
{
    public CombatWeapon? Weapon { get; set; }

    public CombatOffhand? Offhand { get; set; }

    // Bit 1 << BuffKind is set when that spell is shown buffed.
    public uint SpellToggles { get; set; }

    public ulong TarotCards { get; set; }

    public uint CorruptorTicks { get; set; }

    // Kept while unticked.
    public int[] CorruptorCounts { get; } = new int[TreeEffects.CorruptorCount];

    public uint EnchantTicks { get; set; }

    public int[] EnchantCounts { get; } = new int[ItemEffects.EnchantCount];

    public uint GearTicks { get; set; }
}

public static class PlannerChoicesStore
{
    public const string FileName = "TLGPlanner.dat";

    public static PlannerChoices Load(string path, uint characterNumber) => throw new NotImplementedException();

    public static bool Save(string path, uint characterNumber, PlannerChoices choices) =>
        throw new NotImplementedException();
}

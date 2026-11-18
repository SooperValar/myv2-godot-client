using System;
using System.Collections.Generic;
using TheLastGate.Buffs;

namespace TheLastGate.Planner;

public enum SkillScale
{
    None,
    Resist,
    Tactics,
    Immunity,
}

public sealed class BuffModelSkill
{
    // The value the server sends.
    public int ServerTotal { get; set; }

    // Points planned for the skill itself.
    public int Planned { get; set; }

    public IReadOnlyList<AttributeSlot> Attributes { get; set; } = [];

    public SkillScale Scale { get; set; }

    public SkillGroups Groups { get; set; }
}

public sealed class BuffModelInput
{
    public const int BuffCount = 3;

    // The totals the server sends.
    public AttributeAmounts ServerAttributes { get; } = new();

    // The bare values, without gear or buffs.
    public AttributeAmounts BaseAttributes { get; } = new();

    public AttributeAmounts PlannedAttributes { get; } = new();

    // 5, or 6 in hard mode.
    public int Divisor { get; set; } = 5;

    // In hundredths: 100 is 1.00x.
    public int SpellMultiplier { get; set; }

    public int SpellBonus { get; set; }

    public int SpellAptitude { get; set; }

    // A percent.
    public int RaceModifier { get; set; }

    // Enhance and Protect give power / 6 + 3 instead of power / 4 + 4.
    public bool ReducedItemBuffs { get; set; }

    public int ServerWeapon { get; set; }

    public int ServerArmor { get; set; }

    public bool[] SpellKnown { get; } = new bool[BuffCount];

    public BuffModelSkill[] SpellSkills { get; } = [new(), new(), new()];

    // A buff not on the player isn't Known.
    public BuffReading[] Active { get; } = [new(), new(), new()];

    public bool[] ShowBuffed { get; } = new bool[BuffCount];

    public ulong TarotCards { get; set; }

    public int UncappedHitpoints { get; set; }

    public int PlannedHitpoints { get; set; }

    public int UncappedMana { get; set; }

    public int PlannedMana { get; set; }

    public PlannerEffects Effects { get; set; } = new();
}

public sealed class BuffModelResult
{
    // The values the planner shows, plan included.
    public AttributeAmounts Attributes { get; } = new();

    // Aptitude with the planned base Willpower and Intuition.
    public int SpellAptitude { get; set; }

    // Spell Bonus with the shown attributes and planned aptitude.
    public int SpellBonus { get; set; }

    // The player's own planned cast, the second for Bless; 0 for a spell not known.
    public int[] CastPower { get; } = new int[BuffModelInput.BuffCount];

    // The power shown: the stronger of the cast and the active buff while shown, else 0.
    public int[] EffectivePower { get; } = new int[BuffModelInput.BuffCount];

    public int Weapon { get; set; }

    public int Armor { get; set; }
}

public static class BuffModel
{
    public static int BlessBonus(int power, AttributeSlot attribute) => throw new NotImplementedException();

    public static int ItemBonus(int power, bool reduced) => throw new NotImplementedException();

    public static int SkillValue(BuffModelInput input, BuffModelSkill skill, AttributeAmounts attributes) =>
        throw new NotImplementedException();

    public static int SkillValueWithArmor(
        BuffModelInput input,
        BuffModelSkill skill,
        AttributeAmounts attributes,
        int shownArmor) =>
        throw new NotImplementedException();

    public static BuffModelResult Compute(BuffModelInput input) => throw new NotImplementedException();

    public static CombatValues CombatShown(
        BuffModelInput model,
        BuffModelResult buffed,
        CombatModelInput combat,
        IReadOnlyList<BuffModelSkill> combatSkills) =>
        throw new NotImplementedException();
}

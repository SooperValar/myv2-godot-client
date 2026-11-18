using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

public enum CombatSkill
{
    Hand,
    Dagger,
    Sword,
    Axe,
    Staff,
    TwoHanded,
    DualWield,
    Shield,
    Tactics,
    GearMastery,
    Ferocity,
}

public enum CombatWeapon
{
    Unarmed,
    Claw,
    Dagger,
    Sword,
    Axe,
    Staff,
    TwoHanded,
    Spear,
    Greataxe,
}

public enum CombatOffhand
{
    None,
    Shield,
    DualWield,
}

public enum CombatGearClass
{
    Other,
    TemplarOrBraver,
    ArchTemplar,
    Skald,
}

public enum CombatStat
{
    Hit,
    Parry,
    Weapon,
    Armor,
}

public readonly record struct CombatValues(int Hit, int Parry, int Weapon, int Armor)
{
    public int this[CombatStat stat] => throw new NotImplementedException();
}

public sealed class CombatModelInput
{
    public bool[] Known { get; } = new bool[CombatModel.SkillCount];

    public int[] Current { get; } = new int[CombatModel.SkillCount];

    public int[] Shown { get; } = new int[CombatModel.SkillCount];

    public CombatWeapon Weapon { get; set; }

    public CombatOffhand Offhand { get; set; }

    public CombatGearClass GearClass { get; set; }

    public bool Rampart { get; set; }

    public bool Lycanth { get; set; }

    public int EmptySlots { get; set; }

    public int ServerHit { get; set; }

    public int ServerParry { get; set; }

    public int BaseWeapon { get; set; }

    public int BaseArmor { get; set; }

    public bool BestWeaponSkill { get; set; }

    public bool HitFeedsParry { get; set; }

    public bool ParryFeedsHit { get; set; }
}

public static class CombatModel
{
    public const int SkillCount = 11;

    public static IReadOnlyList<string> WeaponNames => throw new NotImplementedException();

    public static IReadOnlyList<string> OffhandNames => throw new NotImplementedException();

    public static IReadOnlyList<string> StatNames => throw new NotImplementedException();

    public static CombatWeapon DefaultWeapon(bool[] known, int[] current) => throw new NotImplementedException();

    public static CombatOffhand DefaultOffhand(bool[] known, int[] current) => throw new NotImplementedException();

    public static CombatValues Compute(CombatModelInput input) => throw new NotImplementedException();

    public static IReadOnlySet<CombatSkill> SkillsAffecting(
        CombatModelInput input,
        CombatStat stat,
        int weaponSkillReach) =>
        throw new NotImplementedException();
}

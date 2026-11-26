using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TheLastGate.Rules;

public enum CombatSkill
{
    Hand,
    Karate,
    Dagger,
    Sword,
    Axe,
    Staff,
    TwoHanded,
    Greataxe,
}

public enum CombatWeapon
{
    Unarmed,
    Dagger,
    Sword,
    Axe,
    Staff,
    TwoHanded,
    Greataxe,
}

public enum CombatOffhand
{
    None,
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
    public int this[CombatStat stat] => stat switch
    {
        CombatStat.Hit => Hit,
        CombatStat.Parry => Parry,
        CombatStat.Weapon => Weapon,
        CombatStat.Armor => Armor,
        _ => throw new UnreachableException(),
    };
}

// Equal to another input with the same numbers.
public sealed class CombatModelInput : IEquatable<CombatModelInput>
{
    public bool[] Known { get; } = new bool[CombatModel.SkillCount];

    // The server's totals.
    public int[] Current { get; } = new int[CombatModel.SkillCount];

    // Planned, with the buffs being shown.
    public int[] Shown { get; } = new int[CombatModel.SkillCount];

    // The weapon the plan is for.
    public CombatWeapon Weapon { get; set; }

    // The weapon in the player's hand, which the server's Hit and Parry Score come from.
    public CombatWeapon HeldWeapon { get; set; }

    public CombatOffhand Offhand { get; set; }

    public int ServerHit { get; set; }

    public int ServerParry { get; set; }

    // Weapon and Armor Value with the buffs being shown.
    public int BaseWeapon { get; set; }

    public int BaseArmor { get; set; }

    public bool Equals(CombatModelInput? other) =>
        other != null
        && (Weapon, HeldWeapon, Offhand, ServerHit, ServerParry, BaseWeapon, BaseArmor)
        == (other.Weapon, other.HeldWeapon, other.Offhand, other.ServerHit, other.ServerParry, other.BaseWeapon,
            other.BaseArmor)
        && Known.SequenceEqual(other.Known)
        && Current.SequenceEqual(other.Current)
        && Shown.SequenceEqual(other.Shown);

    public CombatModelInput Copy()
    {
        var copy = new CombatModelInput
        {
            Weapon = Weapon,
            HeldWeapon = HeldWeapon,
            Offhand = Offhand,
            ServerHit = ServerHit,
            ServerParry = ServerParry,
            BaseWeapon = BaseWeapon,
            BaseArmor = BaseArmor,
        };
        Known.CopyTo(copy.Known, 0);
        Current.CopyTo(copy.Current, 0);
        Shown.CopyTo(copy.Shown, 0);
        return copy;
    }

    public override bool Equals(object? other) => Equals(other as CombatModelInput);

    public override int GetHashCode() => HashCode.Combine(Weapon, HeldWeapon, ServerHit, ServerParry, BaseWeapon);
}

// What the planner shows for Hit, Parry, Weapon Value and Armor Value, by MyV2's get_fight_skill(): Hit and Parry
// Score are both the skill of the weapon in hand (bare hands: the better of Hand to Hand and Karate), less the luck
// penalty, which the server's numbers already carry. Weapon and Armor Value only move with buffs.
public static class CombatModel
{
    public const int SkillCount = 8;

    private const int StatMost = 9999;

    private static readonly string[] WeaponNameTable =
        ["Unarmed", "Dagger", "Sword", "Axe", "Staff", "Two-Handed", "Greataxe"];

    private static readonly string[] OffhandNameTable = ["None"];

    private static readonly string[] StatNameTable = ["Hit Score", "Parry Score", "Weapon Value", "Armor Value"];

    // The server's skill numbers in CombatSkill order, and which weapon each stands for.
    private static readonly (int Number, CombatWeapon Weapon)[] SkillTable =
    [
        (0, CombatWeapon.Unarmed), (1, CombatWeapon.Unarmed), (2, CombatWeapon.Dagger), (3, CombatWeapon.Sword),
        (4, CombatWeapon.Axe), (5, CombatWeapon.Staff), (6, CombatWeapon.TwoHanded), (36, CombatWeapon.Greataxe),
    ];

    public static IReadOnlyList<string> WeaponNames => WeaponNameTable;

    public static IReadOnlyList<string> OffhandNames => OffhandNameTable;

    public static IReadOnlyList<string> StatNames => StatNameTable;

    public static IReadOnlyList<int> SkillNumbers { get; } = SkillTable.Select(entry => entry.Number).ToArray();

    // The weapon the server says the player fights with, by its skill number; anything else is bare hands.
    public static CombatWeapon WeaponForSkill(int skillNumber) =>
        SkillTable.Where(entry => entry.Number == skillNumber).Select(entry => entry.Weapon).FirstOrDefault();

    public static CombatValues Compute(CombatModelInput input)
    {
        var change = WeaponTerm(input.Weapon, input.Shown) - WeaponTerm(input.HeldWeapon, input.Current);
        return new CombatValues(
            Shown(input.ServerHit, change),
            Shown(input.ServerParry, change),
            Shown(input.BaseWeapon, 0),
            Shown(input.BaseArmor, 0));
    }

    // The known skills whose value can move the stat with the chosen weapon.
    public static IReadOnlySet<CombatSkill> SkillsAffecting(CombatModelInput input, CombatStat stat) =>
        stat switch
        {
            CombatStat.Hit or CombatStat.Parry => SkillsOf(input.Weapon).Where(skill => input.Known[(int)skill]).ToHashSet(),
            CombatStat.Weapon or CombatStat.Armor => new HashSet<CombatSkill>(),
            _ => throw new UnreachableException(),
        };

    private static int Shown(int server, int change) => Math.Clamp(server + change, 0, StatMost);

    private static CombatSkill[] SkillsOf(CombatWeapon weapon) => weapon switch
    {
        CombatWeapon.Unarmed => [CombatSkill.Hand, CombatSkill.Karate],
        CombatWeapon.Dagger => [CombatSkill.Dagger],
        CombatWeapon.Sword => [CombatSkill.Sword],
        CombatWeapon.Axe => [CombatSkill.Axe],
        CombatWeapon.Staff => [CombatSkill.Staff],
        CombatWeapon.TwoHanded => [CombatSkill.TwoHanded],
        CombatWeapon.Greataxe => [CombatSkill.Greataxe],
        _ => throw new UnreachableException(),
    };

    // The server reads the skill's total even when it isn't learnt, as the attributes give every skill a value.
    private static int WeaponTerm(CombatWeapon weapon, int[] skills) => SkillsOf(weapon).Max(skill => skills[(int)skill]);
}

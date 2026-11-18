using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

// In the order the Advanced window lists them.
public enum Enchant
{
    MoreBraveness,
    MoreWillpower,
    MoreIntuition,
    MoreAgility,
    MoreStrength,
    MoreAttributes,
    MoreSpellBonus,
    HitToParry,
    ParryToHit,
    MoreArmor,
    MoreResistance,
    ArmorToResistance,
    ArmorToImmunity,
}

// In the order the Advanced window lists them.
public enum Gear
{
    ImprovedVioletGaze,
    ImprovedLionsPaws,
    ImprovedCobaltLance,
    Haladie,
    ImprovedShivasScepter,
    Muramasa,
    ImprovedBarbarianSword,
    ImprovedFellNight,
    ImprovedAjaxBlue,
    TravelersGuide,
    FistsOfTheHeavens,
    GrandeGrimoire,
    ImprovedBlackTachi,
    ImprovedWhiteOdachi,
    Evergreen,
    GestureOfGorn,
    ImprovedNeisClaw,
    Superbia,
    CudgelOfKwai,
    Quicksilver,
    Excalibur,
}

public sealed record EnchantInfo(int ServerNumber, string Name, string OneCopy, bool Counted);

public sealed record GearInfo(string Name, string Effect, bool Modelled);

// The enchants and gear ticked in the Advanced window; an enchant's count is kept while it is unticked.
public sealed class ItemEffectsInput
{
    public uint EnchantTicks { get; set; }

    public int[] EnchantCounts { get; } = new int[ItemEffects.EnchantCount];

    public uint GearTicks { get; set; }
}

public static class ItemEffects
{
    public const int EnchantCount = 13;
    public const int GearCount = 21;
    public const int Cap = 200;

    public static IReadOnlyList<EnchantInfo> Enchants => throw new NotImplementedException();

    public static IReadOnlyList<GearInfo> Gears => throw new NotImplementedException();

    public static int EnchantCopies(ItemEffectsInput input, Enchant enchant) => throw new NotImplementedException();

    public static PlannerEffects Compute(TreeEffectsInput tree, ItemEffectsInput items) =>
        throw new NotImplementedException();
}

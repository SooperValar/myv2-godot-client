using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Planner;

public enum TreeClass
{
    None,
    Seyan,
    ArchTemplar,
    Skald,
    Warrior,
    Sorcerer,
    Summoner,
    ArchHarakim,
    Braver,
    Lycanth,
}

public enum Corruptor
{
    FalseStrength,
    FalseAgility,
    FalseWillpower,
    FalseIntuition,
    FalseBraveness,
    FalseHarrier,
    FalsePragmatism,
    FalseBarbarism,
    FalseCelerity,
    FalseMysticism,
    FalseConcentration,
    FalseVirtuosity,
    FalseRigor,
    FalseFlexibility,
    FalseValor,
    FalseUnbreakable,
    FalseDetermination,
    FalseIndifference,
    FalseConstitution,
    FalseTactician,
    FalseZealotry,
    FalseBrilliance,
    FalseIntrospection,
    FalseWizardry,
    GuardianAngel,
    FalseFlow,
}

public sealed record CorruptorInfo(int ServerNumber, string Name, string OneCopy, bool Modelled);

// The class tree as the player has it, and the corruptor copies ticked in the Advanced window.
public sealed class TreeEffectsInput
{
    public TreeClass TreeClass { get; set; }

    public int TreePoints { get; set; }

    public byte[] TreeNodes { get; } = new byte[PlayerState.TreeNodeCount];

    public uint CorruptorTicks { get; set; }

    public int[] CorruptorCounts { get; } = new int[TreeEffects.CorruptorCount];
}

public static class TreeEffects
{
    public const int CorruptorCount = 26;

    public static IReadOnlyList<CorruptorInfo> Corruptors => throw new NotImplementedException();

    public static TreeClass ClassFromKindred(uint kindred) => throw new NotImplementedException();

    public static bool NodeLearned(int treePoints, int node) => throw new NotImplementedException();

    public static int TreeCopies(TreeEffectsInput input, Corruptor corruptor) => throw new NotImplementedException();

    public static bool NodeInMath(TreeClass treeClass, int node) => throw new NotImplementedException();

    public static Corruptor? CorruptorForServerNumber(int serverNumber) => throw new NotImplementedException();

    public static PlannerEffects Compute(TreeEffectsInput input) => throw new NotImplementedException();
}

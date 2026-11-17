using System.Numerics;

namespace TheLastGate.Windows;

public enum SkillTree
{
    Class,
    Contract,
}

// A tree's value packs the learned nodes into bits 15 down to 4, node 0 at the top, and the unspent points below them.
public static class SkillTrees
{
    public const int Nodes = 12;

    private const int TopBit = 15;
    private const int UnspentMask = 0xF;
    private const int LearnedMask = 0xFFF0;

    public static bool IsLearned(int treeValue, int node) => ((treeValue >> (TopBit - node)) & 1) != 0;

    public static int Unspent(int treeValue) => treeValue & UnspentMask;

    public static int Total(int treeValue) =>
        BitOperations.PopCount((uint)(treeValue & LearnedMask)) + Unspent(treeValue);
}

using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net;

public static class SkillTreePackets
{
    private const int DescribeOffset = 12;
    private const int UnlearnOffset = 24;
    private const int ContractOffset = 36;

    public static byte[] Learn(SkillTree tree, int node) => Tree(tree, node);

    public static byte[] Unlearn(SkillTree tree, int node) => Tree(tree, node + UnlearnOffset);

    public static byte[] Describe(SkillTree tree, int node) => Tree(tree, node + DescribeOffset);

    private static byte[] Tree(SkillTree tree, int value) =>
        ClientPackets.OneValue(ClientCommand.Tree, TreeOffset(tree) + value);

    private static int TreeOffset(SkillTree tree) => tree switch
    {
        SkillTree.Class => 0,
        SkillTree.Contract => ContractOffset,
        _ => throw new UnreachableException(),
    };
}

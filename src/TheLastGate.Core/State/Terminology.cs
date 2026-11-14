using System.Linq;

namespace TheLastGate.State;

public sealed class Terminology
{
    public const int TreeCount = 2;
    public const int NodesPerTree = 12;
    public const int SkillCount = 55;
    public const int MetaStatCount = 89;

    public SkillTreeNode[][] Trees { get; } =
        Enumerable.Range(0, TreeCount).Select(_ => Create<SkillTreeNode>(NodesPerTree)).ToArray();

    public SkillText[] Skills { get; } = Create<SkillText>(SkillCount);
    public MetaStat[] MetaStats { get; } = Create<MetaStat>(MetaStatCount);

    private static TEntry[] Create<TEntry>(int count) where TEntry : new() =>
        Enumerable.Range(0, count).Select(_ => new TEntry()).ToArray();
}

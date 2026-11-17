using System.Linq;

namespace TheLastGate.State;

public sealed class Terminology
{
    public const int TreeCount = 2;
    public const int NodesPerTree = 12;
    public const int SkillCount = 55;
    public const int MetaStatCount = 89;

    private const int ClassPlaceholderIcon = 6601;
    private const int ContractPlaceholderIcon = 6709;

    // Shown until the server sends the trees' real icons and names.
    public SkillTreeNode[][] Trees { get; } =
    [
        Placeholders(ClassPlaceholderIcon, 'p'),
        Placeholders(ContractPlaceholderIcon, 'c'),
    ];

    public SkillText[] Skills { get; } = Create<SkillText>(SkillCount);
    public MetaStat[] MetaStats { get; } = Create<MetaStat>(MetaStatCount);

    private static SkillTreeNode[] Placeholders(int firstIcon, char prefix) =>
        Enumerable.Range(0, NodesPerTree)
            .Select(node => SkillTreeNode.Placeholder(firstIcon + node, $"{prefix}{node + 1:00}"))
            .ToArray();

    private static TEntry[] Create<TEntry>(int count) where TEntry : new() =>
        Enumerable.Range(0, count).Select(_ => new TEntry()).ToArray();
}

using TheLastGate.Characters;

namespace TheLastGate.State;

public sealed class SkillText
{
    public const int NameLength = 30;
    public const int DescriptionLength = 200;
    public const int AttributeCount = 3;

    public int SortKey { get; internal set; }
    public int[] Attributes { get; } = new int[AttributeCount];
    public int Show { get; internal set; }
    public string Name => NulTerminatedText.Read(NameField);
    public string Description => NulTerminatedText.Read(DescriptionField);

    internal byte[] NameField { get; } = new byte[NameLength];
    internal byte[] DescriptionField { get; } = new byte[DescriptionLength];
}

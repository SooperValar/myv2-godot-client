using TheLastGate.Characters;

namespace TheLastGate.State;

public sealed class SkillTreeNode
{
    public const int NameLength = 30;
    public const int DescriptionLength = 50;

    public int Icon { get; internal set; }
    public string Name => NulTerminatedText.Read(NameField);
    public string Description1 => NulTerminatedText.Read(Description1Field);
    public string Description2 => NulTerminatedText.Read(Description2Field);

    internal byte[] NameField { get; } = new byte[NameLength];
    internal byte[] Description1Field { get; } = new byte[DescriptionLength];
    internal byte[] Description2Field { get; } = new byte[DescriptionLength];
}

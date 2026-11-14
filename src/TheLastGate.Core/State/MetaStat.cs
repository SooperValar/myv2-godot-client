using TheLastGate.Characters;

namespace TheLastGate.State;

public sealed class MetaStat
{
    public const int NameLength = 30;
    public const int DescriptionLength = 200;
    public const int AffixLength = 8;

    public string Name => NulTerminatedText.Read(NameField);
    public string Description => NulTerminatedText.Read(DescriptionField);
    public int Value { get; internal set; }
    public int Flag { get; internal set; }
    public string Affix => NulTerminatedText.Read(AffixField);
    public int Font { get; internal set; }
    public int Show { get; internal set; }

    internal byte[] NameField { get; } = new byte[NameLength];
    internal byte[] DescriptionField { get; } = new byte[DescriptionLength];
    internal byte[] AffixField { get; } = new byte[AffixLength];
}

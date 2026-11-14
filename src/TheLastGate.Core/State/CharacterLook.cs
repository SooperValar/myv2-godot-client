using System;
using TheLastGate.Characters;

namespace TheLastGate.State;

public sealed class CharacterLook
{
    public const int WornSlots = 20;
    public const int NameLength = 16;

    public int Number { get; internal set; }
    public int Id { get; internal set; }
    public string Name => NulTerminatedText.Read(NameField);
    public int Sprite { get; internal set; }
    public int Points { get; internal set; }
    public int HitPoints { get; internal set; }
    public int Endurance { get; internal set; }
    public int Mana { get; internal set; }
    public int ActiveHitPoints { get; internal set; }
    public int ActiveEndurance { get; internal set; }
    public int ActiveMana { get; internal set; }
    public int[] Worn { get; } = new int[WornSlots];
    public bool IsAutoLook { get; internal set; }
    public LookFlags Flags { get; internal set; }
    public int HeldItemPrice { get; internal set; }

    internal byte[] NameField { get; } = new byte[NameLength];

    internal CharacterLook Copy() => throw new NotImplementedException();
}

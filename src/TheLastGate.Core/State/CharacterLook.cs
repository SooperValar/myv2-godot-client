using System.Linq;
using TheLastGate.Characters;

namespace TheLastGate.State;

public sealed class CharacterLook
{
    public const int WornSlots = 20;
    public const int NameLength = 16;
    public const int ShopSlots = 62;
    public const int DepotPages = 8;
    public const int DepotSlotsPerPage = 64;

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
    public ShopItem[] ShopItems { get; } = new ShopItem[ShopSlots];

    public DepotItem[][] Depot { get; } =
        Enumerable.Range(0, DepotPages).Select(_ => new DepotItem[DepotSlotsPerPage]).ToArray();

    internal byte[] NameField { get; } = new byte[NameLength];

    internal CharacterLook Copy()
    {
        var copy = new CharacterLook
        {
            Number = Number,
            Id = Id,
            Sprite = Sprite,
            Points = Points,
            HitPoints = HitPoints,
            Endurance = Endurance,
            Mana = Mana,
            ActiveHitPoints = ActiveHitPoints,
            ActiveEndurance = ActiveEndurance,
            ActiveMana = ActiveMana,
            IsAutoLook = IsAutoLook,
            Flags = Flags,
            HeldItemPrice = HeldItemPrice,
        };
        Worn.CopyTo(copy.Worn, 0);
        NameField.CopyTo(copy.NameField, 0);
        ShopItems.CopyTo(copy.ShopItems, 0);
        for (var page = 0; page < DepotPages; page++)
        {
            Depot[page].CopyTo(copy.Depot[page], 0);
        }

        return copy;
    }
}

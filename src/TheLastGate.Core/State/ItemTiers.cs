namespace TheLastGate.State;

// Carried slots are the 40 inventory slots followed by the 20 worn ones, as the server counts them.
public sealed class ItemTiers
{
    public const int CarriedSlots = 60;
    public const int ShopSlots = 62;
    public const int MarkBytes = 124;

    public byte[] CarriedTiers { get; } = new byte[CarriedSlots];

    public byte[] ShopTiers { get; } = new byte[ShopSlots];

    public byte[] CarriedMarks { get; } = new byte[MarkBytes];

    public byte[] ShopMarks { get; } = new byte[MarkBytes];
}

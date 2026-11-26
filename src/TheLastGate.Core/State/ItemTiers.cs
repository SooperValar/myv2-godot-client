namespace TheLastGate.State;

// Carried slots are the 60 backpack slots followed by the 20 worn ones, as the server counts them; both lists come in
// messages as wide as a grave's window.
public sealed class ItemTiers
{
    public const int Slots = CharacterLook.ShopSlots;
    public const int WornStart = PlayerState.InventorySlots;
    public const int MarkBytes = 2 * Slots;

    public byte[] CarriedTiers { get; } = new byte[Slots];

    public byte[] ShopTiers { get; } = new byte[Slots];

    public byte[] CarriedMarks { get; } = new byte[MarkBytes];

    public byte[] ShopMarks { get; } = new byte[MarkBytes];
}

namespace TheLastGate.Net;

public static class InventoryPackets
{
    public static byte[] For(InventoryAction action, int value, int selectedCharacter) =>
        ClientPackets.ThreeValues(ClientCommand.Inventory, (int)action, value, selectedCharacter);
}

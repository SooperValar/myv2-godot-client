using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public static class InventoryPackets
{
    public static byte[] For(InventoryAction action, int value, int selectedCharacter)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)ClientCommand.Inventory;
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1), (int)action);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(5), value);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(9), selectedCharacter);
        return packet;
    }
}

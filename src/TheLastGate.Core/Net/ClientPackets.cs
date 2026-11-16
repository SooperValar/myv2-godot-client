using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public static class ClientPackets
{
    public static byte[] ThreeValues(ClientCommand command, int first, int second, int third)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)command;
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1), first);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(5), second);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(9), third);
        return packet;
    }

    public static byte[] ShortAndInt(ClientCommand command, int first, int second)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)command;
        BinaryPrimitives.WriteInt16LittleEndian(packet.AsSpan(1), (short)first);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(3), second);
        return packet;
    }

    public static byte[] OneValue(ClientCommand command, int value)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)command;
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1), value);
        return packet;
    }
}

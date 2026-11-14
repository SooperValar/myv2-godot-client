using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public static class MapCommandPackets
{
    public static byte[] Move(int worldX, int worldY) => Create(ClientCommand.Move, worldX, worldY);

    public static byte[] Turn(int worldX, int worldY) => Create(ClientCommand.Turn, worldX, worldY);

    private static byte[] Create(ClientCommand command, int worldX, int worldY)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)command;
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(1), (ushort)worldX);
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(3), (uint)worldY);
        return packet;
    }
}

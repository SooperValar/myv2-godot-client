using System;

namespace TheLastGate.Net;

public static class ServerCommandLength
{
    private const int FullPacket = 16;

    private const byte SetMapFlag = 0x80;
    private const int SetMapTileOffsetMask = 0x7F;
    private const int AbsoluteSetMapHeader = 4;
    private const int RelativeSetMapHeader = 2;

    private const int IgnoreHeader = 5;

    private static readonly int[] SetMapFieldSizesByFlagBit = [2, 4, 4, 2, 1, 4, 5, 1];

    public static int Of(ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        var commandId = reader.ReadByte();
        if ((commandId & SetMapFlag) != 0)
        {
            return SetMapLength(commandId & SetMapTileOffsetMask, reader.ReadByte());
        }

        return (ServerCommand)commandId switch
        {
            ServerCommand.SetCharacterName1 => FullPacket,
            ServerCommand.SetCharacterName2 => FullPacket,
            ServerCommand.SetCharacterName3 => FullPacket,
            ServerCommand.SetCharacterMode => 2,
            ServerCommand.SetCharacterAttribute => 8,
            ServerCommand.SetCharacterSkill => 8,
            ServerCommand.SetCharacterHitPoints => 13,
            ServerCommand.SetCharacterEndurance => 13,
            ServerCommand.SetCharacterMana => 13,
            ServerCommand.SetCharacterActiveHitPoints => 3,
            ServerCommand.SetCharacterActiveEndurance => 3,
            ServerCommand.SetCharacterActiveMana => 3,
            ServerCommand.SetCharacterDirection => 2,
            ServerCommand.SetCharacterPoints => 13,
            ServerCommand.SetCharacterGold => 13,
            ServerCommand.SetCharacterItem => 9,
            ServerCommand.SetCharacterWorn => 9,
            ServerCommand.SetCharacterSpell => 9,
            ServerCommand.SetCharacterHeldItem => 5,
            ServerCommand.SetMap3 => LightLength(LightPairs.SetMap3),
            ServerCommand.SetMap4 => LightLength(LightPairs.SetMap4),
            ServerCommand.SetMap5 => LightLength(LightPairs.SetMap5),
            ServerCommand.SetMap6 => LightLength(LightPairs.SetMap6),
            ServerCommand.SetOrigin => 5,
            ServerCommand.Tick => 2,
            >= ServerCommand.Log0 and <= ServerCommand.Log3 => FullPacket,
            ServerCommand.ScrollRight => 1,
            ServerCommand.ScrollLeft => 1,
            ServerCommand.ScrollDown => 1,
            ServerCommand.ScrollUp => 1,
            ServerCommand.ScrollRightDown => 1,
            ServerCommand.ScrollRightUp => 1,
            ServerCommand.ScrollLeftDown => 1,
            ServerCommand.ScrollLeftUp => 1,
            ServerCommand.Look1 => FullPacket,
            ServerCommand.Look2 => FullPacket,
            ServerCommand.Look3 => FullPacket,
            ServerCommand.Look4 => FullPacket,
            ServerCommand.Look5 => FullPacket,
            ServerCommand.Look6 => FullPacket,
            ServerCommand.SetTarget => 13,
            ServerCommand.PlaySound => 13,
            ServerCommand.Exit => FullPacket,
            ServerCommand.Load => 5,
            ServerCommand.Unique => 9,
            ServerCommand.Ignore => IgnoreLength(reader.ReadUInt32()),
            ServerCommand.SpellTime => 24,
            ServerCommand.ItemInfo => 64,
            ServerCommand.TileGlow => 4,
            ServerCommand.Tiers => 84,
            ServerCommand.ItemMarks => 166,
            ServerCommand.WaypointRow => 80,
            ServerCommand.Zone => 41,
            ServerCommand.Meta => 7,
            ServerCommand.Help => 64,
            _ => throw new ProtocolDesyncException($"Server command id {commandId} isn't one a tick can contain."),
        };
    }

    private static int LightLength(int pairedTiles) => 3 + pairedTiles / 2;

    private static int SetMapLength(int tileOffset, byte flags)
    {
        if (flags == 0)
        {
            throw new ProtocolDesyncException("A SetMap command has no flags.");
        }

        var length = tileOffset == 0 ? AbsoluteSetMapHeader : RelativeSetMapHeader;
        for (var bit = 0; bit < SetMapFieldSizesByFlagBit.Length; bit++)
        {
            if ((flags & (1 << bit)) != 0)
            {
                length += SetMapFieldSizesByFlagBit[bit];
            }
        }

        return length;
    }

    private static int IgnoreLength(uint size)
    {
        if (size is < IgnoreHeader or > TickDecompressor.MaxPayloadSize)
        {
            throw new ProtocolDesyncException($"An Ignore command gives its size as {size}.");
        }

        return (int)size;
    }
}

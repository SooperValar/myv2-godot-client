using System;

namespace TheLastGate.Net;

public static class ServerCommandLength
{
    private const int FullPacket = 16;
    private const int TextChunkCommand = 13;

    private const byte SetMapFlag = 0x80;
    private const int SetMapTileOffsetMask = 0x7F;
    private const int AbsoluteSetMapHeader = 4;
    private const int RelativeSetMapHeader = 2;

    private const byte TreeIcon = 0;
    private const byte FirstTreeText = 1;
    private const byte LastTreeText = 13;
    private const byte SkillSort = 14;
    private const byte FirstSkillText = 15;
    private const byte LastSkillText = 37;
    private const byte FirstMetaText = 38;
    private const byte LastMetaText = 60;

    private const int IgnoreHeader = 5;

    private static readonly int[] SetMapFieldSizesByFlagBit = [2, 4, 4, 2, 1, 4, 6, 8];

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
            ServerCommand.SetCharacterWaypoints => 13,
            ServerCommand.SetCharacterTokens => 9,
            ServerCommand.SetCharacterTree => 3,
            ServerCommand.SetCharacterGold => 13,
            ServerCommand.SetCharacterItem => 11,
            ServerCommand.SetCharacterWorn => 10,
            ServerCommand.SetCharacterSpell => 9,
            ServerCommand.SetCharacterHeldItem => 6,
            ServerCommand.SetCharacterLocation1 => 11,
            ServerCommand.SetCharacterLocation2 => 11,
            ServerCommand.SetMap3 => 16,
            ServerCommand.SetMap4 => 6,
            ServerCommand.SetMap5 => 7,
            ServerCommand.SetMap6 => 9,
            ServerCommand.SetOrigin => 5,
            ServerCommand.Tick => 2,
            >= ServerCommand.Log0 and <= ServerCommand.Log9 => FullPacket,
            ServerCommand.TermSkillTree => TreeTermLength(reader.ReadByte()),
            ServerCommand.TermContractTree => TreeTermLength(reader.ReadByte()),
            ServerCommand.TermSkills => SkillTermLength(reader.ReadByte()),
            ServerCommand.TermMeta => MetaTermLength(reader.ReadByte()),
            >= ServerCommand.MessageOfTheDay0 and <= ServerCommand.MessageOfTheDay3 => FullPacket,
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
            ServerCommand.Look7 => 8,
            ServerCommand.Look8 => 8,
            ServerCommand.CloseShop => 1,
            ServerCommand.SetTarget => 13,
            ServerCommand.PlaySound => 13,
            ServerCommand.Exit => FullPacket,
            ServerCommand.Load => 5,
            ServerCommand.Unique => 9,
            ServerCommand.Ignore => IgnoreLength(reader.ReadUInt32()),
            ServerCommand.Waypoints => 1,
            ServerCommand.ShowMessageOfTheDay => 2,
            ServerCommand.ClearBox => 9,
            _ => throw new ProtocolDesyncException($"Server command id {commandId} isn't one a tick can contain."),
        };
    }

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

    private static int TreeTermLength(byte kind) => kind switch
    {
        TreeIcon => 5,
        >= FirstTreeText and <= LastTreeText => TextChunkCommand,
        _ => FullPacket,
    };

    private static int SkillTermLength(byte kind) => kind switch
    {
        SkillSort => 8,
        >= FirstSkillText and <= LastSkillText => TextChunkCommand,
        _ => FullPacket,
    };

    private static int MetaTermLength(byte kind) => kind switch
    {
        >= FirstMetaText and <= LastMetaText => TextChunkCommand,
        _ => FullPacket,
    };

    private static int IgnoreLength(uint size)
    {
        if (size is < IgnoreHeader or > TickDecompressor.MaxPayloadSize)
        {
            throw new ProtocolDesyncException($"An Ignore command gives its size as {size}.");
        }

        return (int)size;
    }
}

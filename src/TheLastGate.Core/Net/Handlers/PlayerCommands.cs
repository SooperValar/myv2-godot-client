using System;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class PlayerCommands
{
    private const int NamePartLength = 15;
    private const int LastNamePartLength = 10;
    private const int LocationPartLength = 10;
    private const int SkillsKeptFromServer = 50;

    public static void Apply(PlayerState player, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        switch ((ServerCommand)reader.ReadByte())
        {
            case ServerCommand.SetCharacterName1:
                reader.ReadBytes(NamePartLength).CopyTo(player.NameField);
                break;
            case ServerCommand.SetCharacterName2:
                reader.ReadBytes(NamePartLength).CopyTo(player.NameField.AsSpan(NamePartLength));
                break;
            case ServerCommand.SetCharacterName3:
                reader.ReadBytes(LastNamePartLength).CopyTo(player.NameField.AsSpan(NamePartLength * 2));
                player.Race = (int)reader.ReadUInt32();
                player.ReportNameReceived();
                break;
            case ServerCommand.SetCharacterMode:
                player.Mode = reader.ReadByte();
                break;
            case ServerCommand.SetCharacterAttribute:
                ReadStat(ref reader, player.Attributes, PlayerState.AttributeCount);
                break;
            case ServerCommand.SetCharacterSkill:
                ReadStat(ref reader, player.Skills, SkillsKeptFromServer);
                break;
            case ServerCommand.SetCharacterHitPoints:
                ReadUnsignedPower(ref reader, player.HitPoints);
                break;
            case ServerCommand.SetCharacterEndurance:
                ReadSignedPower(ref reader, player.Endurance);
                break;
            case ServerCommand.SetCharacterMana:
                ReadSignedPower(ref reader, player.Mana);
                break;
            case ServerCommand.SetCharacterActiveHitPoints:
                player.ActiveHitPoints = reader.ReadUInt16();
                break;
            case ServerCommand.SetCharacterActiveEndurance:
                player.ActiveEndurance = reader.ReadUInt16();
                break;
            case ServerCommand.SetCharacterActiveMana:
                player.ActiveMana = reader.ReadUInt16();
                break;
            case ServerCommand.SetCharacterDirection:
                player.Direction = reader.ReadByte();
                break;
            case ServerCommand.SetCharacterPoints:
                player.Points = (int)reader.ReadUInt32();
                player.PointsTotal = (int)reader.ReadUInt32();
                player.Kindred = reader.ReadUInt32();
                break;
            case ServerCommand.SetCharacterWaypoints:
                player.Waypoints = (int)reader.ReadUInt32();
                player.BlackStrongholdPoints = (int)reader.ReadUInt32();
                player.OsirisPoints = (int)reader.ReadUInt32();
                break;
            case ServerCommand.SetCharacterTokens:
                player.Tokens = (int)reader.ReadUInt32();
                player.TreePoints = reader.ReadUInt16();
                player.OsirisTree = reader.ReadUInt16();
                break;
            case ServerCommand.SetCharacterTree:
                var node = reader.ReadByte();
                var nodeValue = reader.ReadByte();
                if (node < PlayerState.TreeNodeCount)
                {
                    player.TreeNodes[node] = nodeValue;
                }

                break;
            case ServerCommand.SetCharacterGold:
                player.Gold = (int)reader.ReadUInt32();
                player.Armor = (int)reader.ReadUInt32();
                player.Weapon = (int)reader.ReadUInt32();
                break;
            case ServerCommand.SetCharacterLocation1:
                reader.ReadBytes(LocationPartLength).CopyTo(player.LocationField);
                break;
            case ServerCommand.SetCharacterLocation2:
                reader.ReadBytes(LocationPartLength).CopyTo(player.LocationField.AsSpan(LocationPartLength));
                break;
        }
    }

    private static void ReadStat(ref PacketReader reader, int[][] stats, int indexLimit)
    {
        var index = reader.ReadByte();
        var values = reader.ReadBytes(PlayerState.StatValueCount);
        if (index >= indexLimit)
        {
            return;
        }

        for (var slot = 0; slot < PlayerState.StatValueCount; slot++)
        {
            stats[index][slot] = values[slot];
        }
    }

    private static void ReadUnsignedPower(ref PacketReader reader, int[] power)
    {
        for (var slot = 0; slot < PlayerState.StatValueCount; slot++)
        {
            power[slot] = reader.ReadUInt16();
        }
    }

    private static void ReadSignedPower(ref PacketReader reader, int[] power)
    {
        for (var slot = 0; slot < PlayerState.StatValueCount; slot++)
        {
            power[slot] = reader.ReadInt16();
        }
    }
}

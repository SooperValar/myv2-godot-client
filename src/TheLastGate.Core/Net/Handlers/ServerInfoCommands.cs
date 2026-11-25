using System;
using System.Diagnostics;
using TheLastGate.Characters;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class ServerInfoCommands
{
    private const int BuffNameLength = 15;
    private const int ItemInfoTextLength = 58;
    private const byte ItemInfoComplete = 255;
    private const byte CarriedList = 0;
    private const byte WaypointFound = 1;
    private const byte WaypointHere = 2;
    private const int WaypointNameOffset = 8;
    private const int WaypointNameLength = 31;
    private const int WaypointLineLength = 39;
    private const int WaypointLineOffset = 40;

    public static void Apply(GameState state, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        switch ((ServerCommand)reader.ReadByte())
        {
            case ServerCommand.SpellTime:
                ReadSpellTime(state.BuffTimers, ref reader);
                break;
            case ServerCommand.ItemInfo:
                ReadItemInfo(state, ref reader);
                break;
            case ServerCommand.Tiers:
                var tiersFor = reader.ReadByte();
                reader.ReadBytes(tiersFor == CarriedList ? ItemTiers.CarriedSlots : ItemTiers.ShopSlots)
                    .CopyTo(tiersFor == CarriedList ? state.ItemTiers.CarriedTiers : state.ItemTiers.ShopTiers);
                break;
            case ServerCommand.ItemMarks:
                var marksFor = reader.ReadByte();
                reader.ReadBytes(ItemTiers.MarkBytes)
                    .CopyTo(marksFor == CarriedList ? state.ItemTiers.CarriedMarks : state.ItemTiers.ShopMarks);
                break;
            case ServerCommand.WaypointRow:
                ReadWaypointRow(state, command);
                break;
            case ServerCommand.Zone:
                reader.ReadBytes(state.Player.LocationField.Length).CopyTo(state.Player.LocationField);
                break;
            default:
                throw new UnreachableException();
        }
    }

    private static void ReadSpellTime(BuffTimers timers, ref PacketReader reader)
    {
        var slot = reader.ReadByte();
        var secondsLeft = reader.ReadUInt16();
        var isHarmful = reader.ReadByte() != 0;
        var name = NulTerminatedText.Read(reader.ReadBytes(BuffNameLength));
        if (slot < BuffTimers.Slots)
        {
            timers.Set(slot, new BuffTimer(secondsLeft, isHarmful, name));
        }
    }

    private static void ReadItemInfo(GameState state, ref PacketReader reader)
    {
        var where = reader.ReadByte();
        var slot = reader.ReadByte();
        var line = reader.ReadByte();
        var font = reader.ReadByte();
        if (line == ItemInfoComplete)
        {
            state.ItemInfo.Complete(where, slot);
            return;
        }

        state.ItemInfo.Receive(where, slot, line, font, NulTerminatedText.Read(reader.ReadBytes(ItemInfoTextLength)));
    }

    private static void ReadWaypointRow(GameState state, ReadOnlySpan<byte> command)
    {
        var rowIndex = command[1];
        var rowCount = command[2];
        if (rowCount == 0)
        {
            state.WaypointList.Close();
            state.ReportWaypointListChanged();
            return;
        }

        var reader = new PacketReader(command);
        reader.Skip(3);
        var flags = reader.ReadByte();
        var picture = reader.ReadUInt16();
        var number = reader.ReadByte();
        reader.Skip(1);
        var name = NulTerminatedText.Read(reader.ReadBytes(WaypointNameLength));
        reader.Skip(WaypointLineOffset - (WaypointNameOffset + WaypointNameLength));
        var line = NulTerminatedText.Read(reader.ReadBytes(WaypointLineLength));
        var row = new WaypointRow(
            number,
            name,
            line,
            picture,
            (flags & WaypointFound) != 0,
            (flags & WaypointHere) != 0);
        if (state.WaypointList.Receive(rowIndex, rowCount, row))
        {
            state.ReportWaypointListChanged();
        }
    }
}

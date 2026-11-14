using System;
using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class LookCommands
{
    private const int NameBytesSent = 15;

    public static void Apply(GameState state, ReadOnlySpan<byte> command)
    {
        var look = state.PendingLook;
        var reader = new PacketReader(command);
        switch ((ServerCommand)reader.ReadByte())
        {
            case ServerCommand.Look1:
                look.Worn[0] = reader.ReadUInt16();
                look.Worn[2] = reader.ReadUInt16();
                look.Worn[3] = reader.ReadUInt16();
                look.Worn[5] = reader.ReadUInt16();
                look.Worn[6] = reader.ReadUInt16();
                look.Worn[7] = reader.ReadUInt16();
                look.Worn[8] = reader.ReadUInt16();
                look.IsAutoLook = reader.ReadByte() != 0;
                break;
            case ServerCommand.Look2:
                look.Worn[9] = reader.ReadUInt16();
                look.Sprite = reader.ReadUInt16();
                look.Points = (int)reader.ReadUInt32();
                look.HitPoints = (int)reader.ReadUInt32();
                look.Worn[10] = reader.ReadUInt16();
                break;
            case ServerCommand.Look3:
                look.Endurance = reader.ReadUInt16();
                look.ActiveHitPoints = reader.ReadUInt16();
                look.ActiveEndurance = reader.ReadUInt16();
                look.Number = reader.ReadUInt16();
                look.Id = reader.ReadUInt16();
                look.Mana = reader.ReadUInt16();
                look.ActiveMana = reader.ReadUInt16();
                break;
            case ServerCommand.Look4:
                look.Worn[1] = reader.ReadUInt16();
                look.Worn[4] = reader.ReadUInt16();
                look.Flags = (LookFlags)reader.ReadByte();
                look.HeldItemPrice = (int)reader.ReadUInt32();
                look.Worn[11] = reader.ReadUInt16();
                look.Worn[12] = reader.ReadUInt16();
                look.Worn[13] = reader.ReadUInt16();
                break;
            case ServerCommand.Look5:
                reader.ReadBytes(NameBytesSent).CopyTo(look.NameField);
                Finish(state, look);
                break;
            default:
                throw new UnreachableException();
        }
    }

    private static void Finish(GameState state, CharacterLook look)
    {
        if (look.Flags.HasFlag(LookFlags.Shop))
        {
            return;
        }

        if (!look.IsAutoLook)
        {
            state.Look = look.Copy();
            state.ReportLookShown(state.Look);
        }

        state.Names.Remember(look.Number, look.Id, look.Name);
    }
}

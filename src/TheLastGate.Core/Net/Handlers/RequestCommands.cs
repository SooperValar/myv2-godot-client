using System;
using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class RequestCommands
{
    public static void Apply(GameState state, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        switch ((ServerCommand)reader.ReadByte())
        {
            case ServerCommand.PlaySound:
                state.ReportSoundRequested(
                    new SoundRequest((int)reader.ReadUInt32(), (int)reader.ReadUInt32(), (int)reader.ReadUInt32()));
                break;
            case ServerCommand.Exit:
                var code = reader.ReadUInt32();
                state.ReportExitRequested(new ServerExit((int)code, ExitReasons.Describe(code)));
                break;
            case ServerCommand.Load:
                state.ServerLoad = (int)reader.ReadUInt32();
                break;
            case ServerCommand.Unique:
                state.ReportUniqueIdReceived(new UniqueId(reader.ReadUInt32(), reader.ReadUInt32()));
                break;
            case ServerCommand.Ignore:
                break;
            default:
                throw new UnreachableException();
        }
    }
}

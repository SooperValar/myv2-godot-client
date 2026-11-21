using System;
using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class RequestCommands
{
    private const byte MessageOfTheDayWindow = 0;
    private const byte NewPlayerWelcomeWindow = 99;
    private const byte FirstTutorialWindow = 100;

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
            case ServerCommand.Waypoints:
                state.ReportWaypointsOffered();
                break;
            case ServerCommand.ShowMessageOfTheDay:
                state.ReportWindowRequested(WindowFor(reader.ReadByte()));
                break;
            case ServerCommand.ClearBox:
                state.ReportMinimapBoxCleared(
                    new MinimapBox(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                break;
            default:
                throw new UnreachableException();
        }
    }

    private static WindowRequest WindowFor(byte window) => window switch
    {
        >= FirstTutorialWindow => new WindowRequest(WindowKind.Tutorial, window % FirstTutorialWindow),
        NewPlayerWelcomeWindow => new WindowRequest(WindowKind.NewPlayerWelcome, 0),
        MessageOfTheDayWindow => new WindowRequest(WindowKind.MessageOfTheDay, 0),
        _ => new WindowRequest(WindowKind.Book, window),
    };
}

using System.Diagnostics;
using Godot;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Net;

namespace TheLastGate.Game;

public partial class GameScreen : Control
{
    private const string LauncherScene = "res://scenes/Launcher.tscn";

    private readonly ServerConnection connection = new();
    private readonly TickPacer pacer = new();
    private long commandsRead;
    private bool newCharacterSaved;
    private TickPlayback playback = null!;
    private GameSession session = null!;
    private RichTextLabel statusLog = null!;
    private Label tickCounts = null!;

    public override void _Ready()
    {
        statusLog = GetNode<RichTextLabel>("%StatusLog");
        tickCounts = GetNode<Label>("%TickCounts");
        GetNode<Button>("%DisconnectButton").Pressed += ReturnToLauncher;

        if (GameSession.Take() is not { } startedSession)
        {
            SetProcess(false);
            ReturnToLauncher();
            return;
        }

        session = startedSession;
        playback = new TickPlayback(pacer);
        connection.Start(session.Server, session.Handshake);
    }

    public override void _Process(double delta)
    {
        while (connection.TryDequeueMessage(out var message))
        {
            statusLog.AppendText(message + "\n");
        }

        if (!newCharacterSaved && connection.State == ConnectionState.Connected
            && session.Handshake.CreatedKey is { } createdKey)
        {
            SaveNewCharacter(createdKey);
        }

        while (connection.TryDequeueTick(out var tick))
        {
            playback.Receive(tick);
        }

        try
        {
            playback.PlayDueTicks(NowMilliseconds(), ReadCommands, connection.SendCommand);
        }
        catch (ProtocolDesyncException exception)
        {
            statusLog.AppendText($"Lost track of the server's data: {exception.Message}\n");
            connection.Disconnect();
        }

        tickCounts.Text =
            $"Ticks played: {pacer.TicksPlayed}   Queued: {playback.QueuedTicks}   Commands: {commandsRead}";
    }

    public override void _ExitTree() => connection.Dispose();

    private static long NowMilliseconds() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;

    private void ReadCommands(byte[] payload)
    {
        var reader = new TickCommandReader(payload);
        while (reader.TryReadNext(out _))
        {
            commandsRead++;
        }
    }

    private void SaveNewCharacter(LoginKey createdKey)
    {
        newCharacterSaved = true;
        var record = session.Record;
        record.Key = createdKey;
        var settings = record.Settings();
        settings.SetStartingHotbar(CharacterRace.FromNumber(createdKey.Race).Class);
        record.Store(settings);
        CharacterStore.Save(record);
    }

    private void ReturnToLauncher() => GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, LauncherScene);
}

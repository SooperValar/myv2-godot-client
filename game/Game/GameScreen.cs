using System.Diagnostics;
using Godot;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Game;

public partial class GameScreen : Control
{
    private const string LauncherScene = "res://scenes/Launcher.tscn";
    private const int MaximumPowerSlot = 5;

    private readonly ServerConnection connection = new();
    private readonly GameState gameState = new();
    private readonly TickPacer pacer = new();
    private bool newCharacterSaved;
    private TickPlayback playback = null!;
    private TickProcessor processor = null!;
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
        processor = new TickProcessor(gameState);
        gameState.Player.NameReceived += SaveCharacterName;
        gameState.Chat.LineAdded += line => statusLog.AppendText(line.Text + "\n");
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
            playback.PlayDueTicks(NowMilliseconds(), payload => processor.Process(payload), connection.SendCommand);
        }
        catch (ProtocolDesyncException exception)
        {
            statusLog.AppendText($"Lost track of the server's data: {exception.Message}\n");
            connection.Disconnect();
        }

        var player = gameState.Player;
        tickCounts.Text = $"{player.Name} in {player.Location}   "
            + $"HP {player.ActiveHitPoints}/{player.HitPoints[MaximumPowerSlot]}   "
            + $"Ticks played: {pacer.TicksPlayed}   Queued: {playback.QueuedTicks}";
    }

    public override void _ExitTree() => connection.Dispose();

    private static long NowMilliseconds() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;

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

    private void SaveCharacterName()
    {
        var record = session.Record;
        record.Key = record.Key with { Name = gameState.Player.Name, Race = gameState.Player.Race };
        CharacterStore.Save(record);
    }

    private void ReturnToLauncher() => GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, LauncherScene);
}

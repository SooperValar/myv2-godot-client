using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Game;

public partial class GameScreen : Control
{
    private const string LauncherScene = "res://scenes/Launcher.tscn";
    private const int MaximumPowerSlot = 5;
    private const long SpriteCacheBudgetBytes = 256L * 1024 * 1024;
    private const int WorldViewIndex = 1;

    private readonly ServerConnection connection = new();
    private readonly GameState gameState = new();
    private readonly TickPacer pacer = new();
    private readonly HashSet<string> reportedUnusualData = [];
    private bool newCharacterSaved;
    private TickPlayback playback = null!;
    private TickProcessor processor = null!;
    private GameSession session = null!;
    private SpriteLoader? sprites;
    private RichTextLabel statusLog = null!;
    private Label tickCounts = null!;
    private WorldView? worldView;

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
        AddWorldView();
        playback = new TickPlayback(pacer);
        processor = new TickProcessor(gameState);
        gameState.Player.NameReceived += SaveCharacterName;
        gameState.Chat.LineAdded += line => statusLog.AppendText(line.Text + "\n");
        gameState.UniqueIdReceived += UniqueIdStore.Save;
        gameState.ExitRequested += LeaveOnServerExit;
        gameState.UnusualDataSeen += ReportUnusualDataOnce;
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
            var played = false;
            playback.PlayDueTicks(
                NowMilliseconds(),
                payload =>
                {
                    processor.Process(payload);
                    played = true;
                },
                connection.SendCommand);
            if (played)
            {
                worldView?.Refresh();
            }
        }
        catch (ProtocolDesyncException exception)
        {
            statusLog.AppendText($"Lost track of the server's data: {exception.Message}\n");
            connection.Disconnect();
        }

        var player = gameState.Player;
        tickCounts.Text = $"{player.Name} in {player.Location}   "
            + $"HP {player.ActiveHitPoints}/{player.HitPoints[MaximumPowerSlot]}   "
            + $"Ticks played: {pacer.TicksPlayed}   Queued: {playback.QueuedTicks}   "
            + $"World refresh: {worldView?.LastRefreshMilliseconds:F2} ms";
    }

    public override void _ExitTree()
    {
        connection.Dispose();
        sprites?.Dispose();
    }

    private static long NowMilliseconds() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;

    private void AddWorldView()
    {
        try
        {
            sprites = new SpriteLoader(DataPaths.Root(), SpriteCacheBudgetBytes);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            statusLog.AppendText($"Couldn't open the sprite archives: {exception.Message}\n");
            return;
        }

        worldView = new WorldView
        {
            State = gameState,
            Sprites = sprites,
            SendCommand = connection.SendCommand,
            PickTileDrawnUnderMouse = ClientSettings.Load().PickTileDrawnUnderMouse,
        };
        AddChild(worldView);
        MoveChild(worldView, WorldViewIndex);
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

    private void SaveCharacterName()
    {
        var record = session.Record;
        record.Key = record.Key with { Name = gameState.Player.Name, Race = gameState.Player.Race };
        CharacterStore.Save(record);
    }

    private void ReportUnusualDataOnce(string description)
    {
        if (reportedUnusualData.Add(description))
        {
            GD.PushWarning(description);
        }
    }

    private void LeaveOnServerExit(ServerExit exit)
    {
        statusLog.AppendText($"Server demands exit: {exit.Reason}\n");
        connection.Disconnect();
    }

    private void ReturnToLauncher() => GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, LauncherScene);
}

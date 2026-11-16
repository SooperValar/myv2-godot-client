using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Hud;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Game;

public partial class GameScreen : Control
{
    private const string LauncherScene = "res://Launcher/launcher.tscn";
    private const long SpriteCacheBudgetBytes = 256L * 1024 * 1024;
    private const int WorldViewIndex = 1;
    private const int ErrorFont = 0;
    private const int StatusFont = 1;

    private readonly ServerConnection connection = new();
    private readonly GameState gameState = new();
    private readonly TickPacer pacer = new();
    private readonly HashSet<string> reportedUnusualData = [];
    private HudView hudView = null!;
    private bool newCharacterSaved;
    private TickPlayback playback = null!;
    private TickProcessor processor = null!;
    private GameSession session = null!;
    private SpriteLoader? sprites;
    private WorldView? worldView;

    public override void _Ready()
    {
        hudView = GetNode<HudView>("%HudView");
        hudView.LeaveRequested += ReturnToLauncher;

        if (GameSession.Take() is not { } startedSession)
        {
            SetProcess(false);
            ReturnToLauncher();
            return;
        }

        session = startedSession;
        CursorShapes.Register(ClientSettings.Load().CursorScale);
        AddWorldView();
        playback = new TickPlayback(pacer);
        processor = new TickProcessor(gameState);
        gameState.Player.NameReceived += SaveCharacterName;
        gameState.UniqueIdReceived += UniqueIdStore.Save;
        gameState.ExitRequested += LeaveOnServerExit;
        gameState.UnusualDataSeen += ReportUnusualDataOnce;
        connection.Start(session.Server, session.Handshake);
    }

    public override void _Process(double delta)
    {
        while (connection.TryDequeueMessage(out var message))
        {
            gameState.Chat.Add(new LogLine(message, StatusFont));
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
                hudView.Refresh(worldView?.SelectedCharacter ?? 0);
            }
        }
        catch (ProtocolDesyncException exception)
        {
            gameState.Chat.Add(new LogLine($"Lost track of the server's data: {exception.Message}", ErrorFont));
            connection.Disconnect();
        }
    }

    public override void _ExitTree()
    {
        CursorShapes.Clear();
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
            GD.PushError($"Couldn't open the sprite archives: {exception.Message}");
            return;
        }

        var fonts = new ClassicFonts(sprites);
        var characterSettings = session.Record.Settings();
        var clientSettings = ClientSettings.Load();
        worldView = new WorldView
        {
            State = gameState,
            Sprites = sprites,
            Fonts = fonts,
            SendCommand = connection.SendCommand,
            PickTileDrawnUnderMouse = clientSettings.PickTileDrawnUnderMouse,
            ShowShadows = clientSettings.Shadows,
            ShowNames = characterSettings.ShowNames,
            ShowHealthPercent = characterSettings.ShowHealthPercent,
            ShowHealthBars = characterSettings.ShowHealthBars,
            HideWalls = characterSettings.HideWalls,
        };
        AddChild(worldView);
        MoveChild(worldView, WorldViewIndex);
        hudView.DarkMode = clientSettings.DarkMode;
        hudView.Sprites = sprites;
        hudView.Settings = characterSettings;
        hudView.SettingsChanged += () => ApplyCharacterSettings(characterSettings);
        hudView.Show(gameState, fonts, connection.SendCommand);
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

    private void ApplyCharacterSettings(CharacterSettings settings)
    {
        if (worldView != null)
        {
            worldView.ShowNames = settings.ShowNames;
            worldView.ShowHealthPercent = settings.ShowHealthPercent;
            worldView.ShowHealthBars = settings.ShowHealthBars;
            worldView.HideWalls = settings.HideWalls;
            worldView.Refresh();
        }

        session.Record.Store(settings);
        CharacterStore.Save(session.Record);
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
        gameState.Chat.Add(new LogLine(" ", StatusFont));
        gameState.Chat.Add(new LogLine($"EXIT: {exit.Reason}", StatusFont));
        connection.Disconnect();
        hudView.ServerEnded(exit.Code);
    }

    private void ReturnToLauncher() => GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, LauncherScene);
}

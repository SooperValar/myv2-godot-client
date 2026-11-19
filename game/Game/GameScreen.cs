using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Audio;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Hud;
using TheLastGate.Items;
using TheLastGate.Map;
using TheLastGate.Net;
using TheLastGate.Planner;
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

    private readonly GameState gameState = new();
    private readonly NavigationKeys navigationKeys = new();
    private readonly TickPacer pacer = new();
    private readonly HashSet<string> reportedUnusualData = [];
    private ServerConnection connection = null!;
    private ExploredMapKeeper? exploredMap;
    private HudView hudView = null!;
    private ConnectionState lastConnectionState;
    private MapNotesKeeper? mapNotes;
    private bool newCharacterSaved;
    private TickPlayback playback = null!;
    private TickProcessor processor = null!;
    private ScreenGammaLayer? screenGamma;
    private GameSession session = null!;
    private SoundEffects soundEffects = null!;
    private SpriteLoader? sprites;
    private UserDataUpload? userDataUpload;
    private WorldView? worldView;

    public override void _Ready()
    {
        navigationKeys.TurnOff();
        hudView = GetNode<HudView>("%HudView");
        soundEffects = GetNode<SoundEffects>("%SoundEffects");
        hudView.LeaveRequested += ReturnToLauncher;

        if (GameSession.Take() is not { } startedSession)
        {
            SetProcess(false);
            ReturnToLauncher();
            return;
        }

        session = startedSession;
        connection = startedSession.Connection;
        CursorShapes.Register(ClientSettings.Load().CursorScale);
        OpenExploredMap();
        AddWorldView();
        playback = new TickPlayback(pacer);
        processor = new TickProcessor(gameState);

        userDataUpload = UserDataUpload.For(session.Record, gameState.Chat);
        gameState.Player.NameReceived += SaveCharacterName;
        gameState.UniqueIdReceived += UniqueIdStore.Save;
        gameState.ExitRequested += LeaveOnServerExit;
        gameState.UnusualDataSeen += ReportUnusualDataOnce;
    }

    public override void _Process(double delta)
    {
        while (connection.TryDequeueMessage(out var message))
        {
            gameState.Chat.Add(new LogLine(message, StatusFont));
        }

        SaveMapsWhenTheConnectionEnds();
        exploredMap?.Advance(TimeSpan.FromSeconds(delta));

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
            var ticksPlayed = 0;
            playback.PlayDueTicks(
                NowMilliseconds(),
                payload =>
                {
                    processor.Process(payload);
                    ticksPlayed++;
                    SendUserData();
                },
                connection.SendCommand);
            if (ticksPlayed > 0)
            {
                if (sprites != null)
                {
                    exploredMap?.See(gameState.Map, sprites);
                }

                worldView?.Refresh();
                worldView?.RecordMovement(ticksPlayed, TickMilliseconds());
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
        navigationKeys.TurnOn();
        exploredMap?.Save();
        mapNotes?.Save();
        CursorShapes.Clear();
        connection?.Dispose();
        sprites?.Dispose();
    }

    private void OpenExploredMap()
    {
        var keeper = new ExploredMapKeeper(session.Server);
        gameState.MinimapBoxCleared += keeper.Recorder.ClearBox;
        gameState.Chat.LineAdded += line =>
            keeper.Recorder.NoteLogLine(line.Text, ExploredMapKeeper.ViewCentreCharacter(gameState.Map));
        exploredMap = keeper;
        mapNotes = new MapNotesKeeper(session.Server);
    }

    // The current client saves its maps when the connection fails, in case the game is closed straight after.
    private void SaveMapsWhenTheConnectionEnds()
    {
        var connectionState = connection.State;
        if (connectionState != lastConnectionState
            && connectionState is ConnectionState.Closed or ConnectionState.Failed)
        {
            exploredMap?.Save();
            mapNotes?.Save();
        }

        lastConnectionState = connectionState;
    }

    private void ApplyClientSettings(ClientSettings settings)
    {
        soundEffects.Enabled = settings.SoundEnabled;
        soundEffects.Volume = settings.SoundVolume;
        CursorShapes.Register(settings.CursorScale);
        gameState.Tooltips.Enabled = settings.ItemTooltips;
        gameState.UseQueue.Enabled = settings.UseQueue;
        gameState.GiveMore.Enabled = settings.GiveMore;
        pacer.Adaptive = settings.AdaptivePacing;
        screenGamma?.ShowGamma(settings.ScreenGamma);
        if (worldView != null)
        {
            worldView.GlideMovement = settings.GlideMovement;
            worldView.PickTileDrawnUnderMouse = settings.PickTileDrawnUnderMouse;
            worldView.WorldGamma = settings.WorldGamma;
        }
    }

    private static long NowMilliseconds() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;

    private double TickMilliseconds() =>
        pacer.MeasuredPeriodMilliseconds >= 1 ? pacer.MeasuredPeriodMilliseconds : TickPacer.NominalTickMilliseconds;

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
            SendCommand = SendAction,
            PlayClick = soundEffects.PlayClick,
            PickTileDrawnUnderMouse = clientSettings.PickTileDrawnUnderMouse,
            GlideMovement = clientSettings.GlideMovement,
            WorldGamma = clientSettings.WorldGamma,
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
        hudView.ClientSettings = clientSettings;
        hudView.PlannerChoices = new PlannerChoicesKeeper(() => session.Record.Key.UserNumber);
        hudView.ClientSettingsChanged += () => ApplyClientSettings(clientSettings);
        hudView.SettingsChanged += () => ApplyCharacterSettings(characterSettings);
        hudView.PlayClick = soundEffects.PlayClick;
        hudView.ReadDebugFacts = ReadDebugFacts;
        hudView.ExploredMap = exploredMap?.Recorder.Map;
        hudView.NotesSetAsideFile = mapNotes is { WasSetAside: true } ? mapNotes.FileName : null;
        hudView.MapNotes = mapNotes?.Notes;
        hudView.SaveMapNotes = mapNotes == null ? null : mapNotes.Save;
        hudView.Show(gameState, fonts, SendAction);
        soundEffects.Start(DataPaths.Root(), clientSettings.SoundEnabled, clientSettings.SoundVolume);
        gameState.SoundRequested += soundEffects.PlayServerSound;
        gameState.Tooltips.SendLook = connection.SendCommand;
        gameState.Tooltips.Enabled = clientSettings.ItemTooltips;
        gameState.UseQueue.Enabled = clientSettings.UseQueue;

        // A queued use has already clicked when it was queued.
        gameState.UseQueue.Send = connection.SendCommand;
        gameState.GiveMore.Enabled = clientSettings.GiveMore;
        pacer.Adaptive = clientSettings.AdaptivePacing;
        gameState.GiveMore.Send = SendAction;
        gameState.ShopRefresh.Send = connection.SendCommand;
        gameState.AutoWalk.Send = connection.SendCommand;
        gameState.BuffWatch.Say = text =>
        {
            foreach (var packet in ChatPackets.Say(text))
            {
                connection.SendCommand(packet);
            }
        };
        screenGamma = new ScreenGammaLayer();
        AddChild(screenGamma);
        screenGamma.ShowGamma(clientSettings.ScreenGamma);
    }

    private DebugFacts ReadDebugFacts() => new(
        typeof(GameScreen).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "",
        session.Handshake.ServerVersion,
        Engine.GetVersionInfo()["string"].AsString(),
        RenderingServer.GetVideoAdapterName(),
        RenderingServer.GetVideoAdapterVendor(),
        RenderingServer.GetVideoAdapterApiVersion(),
        playback.QueuedTicks,
        pacer.BufferTarget,
        pacer.MeasuredPeriodMilliseconds);

    private void SendAction(byte[] packet)
    {
        soundEffects.PlayClickFor(packet);
        gameState.UseQueue.Sent(packet);
        if (ItemTooltips.IsManualLook(packet))
        {
            gameState.Tooltips.ManualLookSent();
        }

        // Using or looking at something lets the next window the server opens stay open.
        if ((ClientCommand)packet[0] is ClientCommand.Use or ClientCommand.Look)
        {
            gameState.Pause.Cancel();
        }

        connection.SendCommand(packet);
    }

    // Once sent, the character is no longer changed, so the next login doesn't send it again.
    private void SendUserData()
    {
        if (userDataUpload?.Tick() is not { } packet)
        {
            return;
        }

        connection.SendCommand(packet);
        if (userDataUpload.Done)
        {
            userDataUpload = null;
            session.Record.Changed = false;
            CharacterStore.Save(session.Record);
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

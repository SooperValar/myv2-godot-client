using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using TheLastGate.Characters;
using TheLastGate.Net;

namespace TheLastGate.Config;

public sealed class ClientSettings
{
    private const int MaxSoundVolume = 10;
    private const float SmallestCursorScale = 0.5f;
    private const float LargestCursorScale = 8f;

    private const string FilePath = "user://settings.cfg";
    private const string DisplaySection = "display";
    private const string AudioSection = "audio";
    private const string LauncherSection = "launcher";
    private const string ServerSection = "server";
    private const string WorldSection = "world";

    private readonly ConfigFile file;

    private ClientSettings(ConfigFile file)
    {
        this.file = file;
        Windowed = file.GetValue(DisplaySection, "windowed", true).AsBool();
        Shadows = file.GetValue(DisplaySection, "shadows", true).AsBool();
        DarkMode = file.GetValue(DisplaySection, "dark_mode", false).AsBool();
        SoundEnabled = file.GetValue(AudioSection, "sound_enabled", true).AsBool();
        SoundVolume = Math.Clamp(
            file.GetValue(AudioSection, "sound_volume", MaxSoundVolume).AsInt32(),
            0,
            MaxSoundVolume);

        CharacterHistory = new CharacterHistory(
            file.HasSectionKey(LauncherSection, "character_files") ? SavedCharacters(file) : SdlClientCharacters());
        CharacterHistory.RemoveMissingFiles();

        // Never written by the client: a [server] section added by hand points it at another server.
        Server = new ServerAddress(
            file.GetValue(ServerSection, "host", ServerAddress.Live.Host).AsString(),
            file.GetValue(ServerSection, "proxy", ServerAddress.Live.Proxy).AsString(),
            file.GetValue(ServerSection, "port", ServerAddress.Live.Port).AsInt32());

        // Also hand-added until the Options window exists: "true" picks the tile drawn under the mouse while the
        // player is mid-step, where the current client picks as if the map hadn't moved.
        PickTileDrawnUnderMouse = file.GetValue(WorldSection, "pick_tile_drawn_under_mouse", false).AsBool();

        // Also hand-added: "true" glides characters and the map between ticks, where the current client snaps.
        GlideMovement = file.GetValue(WorldSection, "glide_movement", false).AsBool();
        CursorScale = Math.Clamp(
            file.GetValue(DisplaySection, "cursor_scale", 1f).AsSingle(),
            SmallestCursorScale,
            LargestCursorScale);
    }

    public bool Windowed { get; set; }
    public bool Shadows { get; set; }
    public bool DarkMode { get; set; }
    public bool SoundEnabled { get; set; }
    public int SoundVolume { get; set; }
    public CharacterHistory CharacterHistory { get; }
    public ServerAddress Server { get; }
    public bool PickTileDrawnUnderMouse { get; }
    public bool GlideMovement { get; }
    public float CursorScale { get; }

    public static ClientSettings Load()
    {
        var file = new ConfigFile();
        file.Load(FilePath);
        return new ClientSettings(file);
    }

    public Error Save()
    {
        file.SetValue(DisplaySection, "windowed", Windowed);
        file.SetValue(DisplaySection, "shadows", Shadows);
        file.SetValue(DisplaySection, "dark_mode", DarkMode);
        file.SetValue(AudioSection, "sound_enabled", SoundEnabled);
        file.SetValue(AudioSection, "sound_volume", SoundVolume);
        file.SetValue(
            LauncherSection,
            "character_names",
            CharacterHistory.Entries.Select(entry => entry.Name).ToArray());
        file.SetValue(
            LauncherSection,
            "character_files",
            CharacterHistory.Entries.Select(entry => entry.FilePath).ToArray());
        return file.Save(FilePath);
    }

    private static IEnumerable<PreviousCharacter> SavedCharacters(ConfigFile file)
    {
        var names = file.GetValue(LauncherSection, "character_names", Array.Empty<string>()).AsStringArray();
        var filePaths = file.GetValue(LauncherSection, "character_files", Array.Empty<string>()).AsStringArray();
        return filePaths.Select((filePath, entryIndex) => new PreviousCharacter(
            entryIndex < names.Length ? names[entryIndex] : "",
            filePath));
    }

    private static IEnumerable<PreviousCharacter> SdlClientCharacters()
    {
        var listPath = SdlClientCharacterList.FilePath();
        if (!File.Exists(listPath))
        {
            return [];
        }

        try
        {
            return SdlClientCharacterList.Parse(File.ReadAllBytes(listPath));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            GD.PushWarning($"Couldn't read the SDL client's character list: {exception.Message}");
            return [];
        }
    }
}

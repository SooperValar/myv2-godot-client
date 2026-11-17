using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot;
using TheLastGate.Characters;
using TheLastGate.Hotkeys;
using TheLastGate.Net;
using TheLastGate.Windows;

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
    private const string KeybindsSection = "keybinds";
    private const string GeneralSection = "general";

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

        EscapeClosesWindowsFirst = file.GetValue(GeneralSection, "escape_closes_windows_first", true).AsBool();
        ExpandedQuickStats = file.GetValue(GeneralSection, "expanded_quick_stats", true).AsBool();
        ItemTooltips = file.GetValue(GeneralSection, "item_tooltips", true).AsBool();
        ItemTooltipsAlways = file.GetValue(GeneralSection, "item_tooltips_always", true).AsBool();
        PickTileDrawnUnderMouse = file.GetValue(WorldSection, "pick_tile_drawn_under_mouse", false).AsBool();
        GlideMovement = file.GetValue(WorldSection, "glide_movement", false).AsBool();
        KeyBindings.Load(SavedBindings(file));
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
    public bool EscapeClosesWindowsFirst { get; set; }
    public bool ExpandedQuickStats { get; set; }
    public bool ItemTooltips { get; set; }
    public bool ItemTooltipsAlways { get; set; }
    public bool PickTileDrawnUnderMouse { get; set; }
    public bool GlideMovement { get; set; }
    public KeyBindings KeyBindings { get; } = new();
    public float CursorScale { get; set; }

    public bool this[GeneralOption option]
    {
        get => option switch
        {
            GeneralOption.EscapeClosesWindowsFirst => EscapeClosesWindowsFirst,
            GeneralOption.ExpandedQuickStats => ExpandedQuickStats,
            GeneralOption.ItemTooltips => ItemTooltips,
            GeneralOption.ItemTooltipsAlways => ItemTooltipsAlways,
            GeneralOption.GlideMovement => GlideMovement,
            GeneralOption.PickTileDrawnUnderMouse => PickTileDrawnUnderMouse,
            _ => throw new UnreachableException(),
        };
        set
        {
            switch (option)
            {
                case GeneralOption.EscapeClosesWindowsFirst:
                    EscapeClosesWindowsFirst = value;
                    break;
                case GeneralOption.ExpandedQuickStats:
                    ExpandedQuickStats = value;
                    break;
                case GeneralOption.ItemTooltips:
                    ItemTooltips = value;
                    break;
                case GeneralOption.ItemTooltipsAlways:
                    ItemTooltipsAlways = value;
                    break;
                case GeneralOption.GlideMovement:
                    GlideMovement = value;
                    break;
                case GeneralOption.PickTileDrawnUnderMouse:
                    PickTileDrawnUnderMouse = value;
                    break;
                default:
                    throw new UnreachableException();
            }
        }
    }

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
        file.SetValue(DisplaySection, "cursor_scale", CursorScale);
        file.SetValue(GeneralSection, "escape_closes_windows_first", EscapeClosesWindowsFirst);
        file.SetValue(GeneralSection, "expanded_quick_stats", ExpandedQuickStats);
        file.SetValue(GeneralSection, "item_tooltips", ItemTooltips);
        file.SetValue(GeneralSection, "item_tooltips_always", ItemTooltipsAlways);
        file.SetValue(WorldSection, "pick_tile_drawn_under_mouse", PickTileDrawnUnderMouse);
        file.SetValue(WorldSection, "glide_movement", GlideMovement);
        file.SetValue(
            LauncherSection,
            "character_names",
            CharacterHistory.Entries.Select(entry => entry.Name).ToArray());
        file.SetValue(
            LauncherSection,
            "character_files",
            CharacterHistory.Entries.Select(entry => entry.FilePath).ToArray());
        foreach (var (id, keys) in KeyBindings.Saved())
        {
            file.SetValue(KeybindsSection, id, keys);
        }

        return file.Save(FilePath);
    }

    private static Dictionary<string, string> SavedBindings(ConfigFile file) =>
        file.HasSection(KeybindsSection)
            ? file.GetSectionKeys(KeybindsSection)
                .ToDictionary(id => id, id => file.GetValue(KeybindsSection, id, "").AsString())
            : [];

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

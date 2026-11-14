using System;
using System.Linq;
using Godot;
using TheLastGate.Characters;

namespace TheLastGate.Config;

public sealed class ClientSettings
{
    private const int MaxSoundVolume = 10;

    private const string FilePath = "user://settings.cfg";
    private const string DisplaySection = "display";
    private const string AudioSection = "audio";
    private const string LauncherSection = "launcher";

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

        var names = file.GetValue(LauncherSection, "character_names", Array.Empty<string>()).AsStringArray();
        var filePaths = file.GetValue(LauncherSection, "character_files", Array.Empty<string>()).AsStringArray();
        CharacterHistory = new CharacterHistory(
            filePaths.Select((filePath, entryIndex) => new PreviousCharacter(
                entryIndex < names.Length ? names[entryIndex] : "",
                filePath)));
        CharacterHistory.RemoveMissingFiles();
    }

    private bool Windowed { get; }
    private bool Shadows { get; }
    private bool DarkMode { get; }
    private bool SoundEnabled { get; }
    private int SoundVolume { get; }
    private CharacterHistory CharacterHistory { get; }

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
}

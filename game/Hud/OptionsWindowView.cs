using System;
using Godot;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// The current client's Options window: a Dear ImGui window with game-sprite buttons, rebuilt as themed controls.
[Tool]
public partial class OptionsWindowView : Panel
{
    private const int ButtonSprite = 18129;

    private CheckBox SoundEnabled => GetNode<CheckBox>("Pages/Audio/SoundEnabled");

    private HSlider Volume => GetNode<HSlider>("Pages/Audio/VolumeRow/Volume");

    public event Action<OptionsTab>? TabChosen;

    public event Action? CloseChosen;

    public event Action<bool>? SoundEnabledChanged;

    public event Action<int>? SoundVolumeChanged;

    public override void _Ready()
    {
        ApplyButtonSprite();
        if (Engine.IsEditorHint())
        {
            return;
        }

        foreach (var tab in Enum.GetValues<OptionsTab>())
        {
            TabButton(tab).Pressed += () => TabChosen?.Invoke(tab);
        }

        GetNode<Button>("CloseButton").Pressed += () => CloseChosen?.Invoke();
        SoundEnabled.Toggled += enabled => SoundEnabledChanged?.Invoke(enabled);
        Volume.ValueChanged += volume =>
        {
            ShowVolume((int)volume);
            SoundVolumeChanged?.Invoke((int)volume);
        };
    }

    public void ShowOptions(OptionsWindow window, bool soundEnabled, int soundVolume)
    {
        Visible = window.IsOpen;
        if (!Visible)
        {
            return;
        }

        TabButton(window.Tab).SetPressedNoSignal(true);
        GetNode<TabContainer>("Pages").CurrentTab = (int)window.Tab;
        SoundEnabled.SetPressedNoSignal(soundEnabled);
        Volume.SetValueNoSignal(soundVolume);
        ShowVolume(soundVolume);
    }

    // Buttons draw the game's button sprite, which lives in the sprite archives rather than in the project.
    public void ApplyButtonSprite()
    {
        if (Theme == null || HudView.Containing(this)?.Sprites?.GetTexture(ButtonSprite) is not { } texture)
        {
            return;
        }

        foreach (var type in Theme.GetStyleboxTypeList())
        {
            foreach (var name in Theme.GetStyleboxList(type))
            {
                if (Theme.GetStylebox(name, type) is StyleBoxTexture box)
                {
                    box.Texture = texture;
                }
            }
        }
    }

    private Button TabButton(OptionsTab tab) => GetNode<Button>($"Tabs/{tab}Tab");

    private void ShowVolume(int volume) =>
        GetNode<Label>("Pages/Audio/VolumeRow/Volume/Value").Text = volume.ToString();
}

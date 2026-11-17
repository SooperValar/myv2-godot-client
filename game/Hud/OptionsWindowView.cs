using System;
using System.Globalization;
using System.Linq;
using Godot;
using TheLastGate.Config;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// The current client's Options window: a Dear ImGui window with game-sprite buttons, rebuilt as themed controls.
[Tool]
public partial class OptionsWindowView : Panel
{
    private const int ButtonSprite = 18129;
    private const string OptionRowScenePath = "res://Hud/option_row.tscn";

    private CheckBox SoundEnabled => GetNode<CheckBox>("Pages/Audio/SoundEnabled");

    private HSlider Volume => GetNode<HSlider>("Pages/Audio/VolumeRow/Volume");

    private HSlider CursorScale => GetNode<HSlider>("Pages/General/Rows/CursorRow/CursorScale");

    public event Action<OptionsTab>? TabChosen;

    public event Action? CloseChosen;

    public event Action<bool>? SoundEnabledChanged;

    public event Action<int>? SoundVolumeChanged;

    public event Action<GeneralOption, bool>? OptionChanged;

    public event Action<float>? CursorScaleChanged;

    public override void _Ready()
    {
        ApplyButtonSprite();
        AddOptionRows();
        if (Engine.IsEditorHint())
        {
            return;
        }

        foreach (var tab in Enum.GetValues<OptionsTab>())
        {
            TabButton(tab).Pressed += () => TabChosen?.Invoke(tab);
        }

        foreach (var row in GeneralOptions.Rows)
        {
            OptionBox(row.Option).Toggled += isOn => OptionChanged?.Invoke(row.Option, isOn);
        }

        GetNode<Button>("CloseButton").Pressed += () => CloseChosen?.Invoke();
        SoundEnabled.Toggled += enabled => SoundEnabledChanged?.Invoke(enabled);
        Volume.ValueChanged += volume =>
        {
            ShowVolume((int)volume);
            SoundVolumeChanged?.Invoke((int)volume);
        };
        CursorScale.ValueChanged += scale =>
        {
            ShowCursorScale((float)scale);
            CursorScaleChanged?.Invoke((float)scale);
        };
    }

    public void ShowOptions(OptionsWindow window, ClientSettings? settings)
    {
        Visible = window.IsOpen;
        if (!Visible || settings == null)
        {
            return;
        }

        TabButton(window.Tab).SetPressedNoSignal(true);
        GetNode<TabContainer>("Pages").CurrentTab = (int)window.Tab;
        foreach (var row in GeneralOptions.Rows)
        {
            OptionBox(row.Option).SetPressedNoSignal(settings[row.Option]);
        }

        CursorScale.SetValueNoSignal(settings.CursorScale);
        ShowCursorScale(settings.CursorScale);
        SoundEnabled.SetPressedNoSignal(settings.SoundEnabled);
        Volume.SetValueNoSignal(settings.SoundVolume);
        ShowVolume(settings.SoundVolume);
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

    // The rows come from the Core table so their wording is kept in one place; they are never saved into the scene.
    private void AddOptionRows()
    {
        var rowScene = GD.Load<PackedScene>(OptionRowScenePath);
        foreach (var row in GeneralOptions.Rows)
        {
            var rowNode = rowScene.Instantiate<Control>();
            rowNode.Name = row.Option.ToString();
            rowNode.GetNode<Label>("Text").Text = row.Label;
            GetNode(row.GodotOnly ? "Pages/General/Rows/GodotOptions" : "Pages/General/Rows/ClientOptions")
                .AddChild(rowNode);
        }
    }

    private CheckBox OptionBox(GeneralOption option)
    {
        var row = GeneralOptions.Rows.First(row => row.Option == option);
        var group = row.GodotOnly ? "GodotOptions" : "ClientOptions";
        return GetNode<CheckBox>($"Pages/General/Rows/{group}/{option}/Box");
    }

    private Button TabButton(OptionsTab tab) => GetNode<Button>($"Tabs/{tab}Tab");

    private void ShowVolume(int volume) =>
        GetNode<Label>("Pages/Audio/VolumeRow/Volume/Value").Text = volume.ToString(CultureInfo.InvariantCulture);

    private void ShowCursorScale(float scale) =>
        GetNode<Label>("Pages/General/Rows/CursorRow/CursorScale/Value").Text =
            scale.ToString("0.0", CultureInfo.InvariantCulture) + "x";
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Hotkeys;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Hud;

// The current client's Options window: a Dear ImGui window with game-sprite buttons, rebuilt as themed controls.
[Tool]
public partial class OptionsWindowView : Panel
{
    private const int ButtonSprite = 18129;
    private const string OptionRowScenePath = "res://Hud/option_row.tscn";
    private const string KeybindSectionScenePath = "res://Hud/keybind_section.tscn";
    private const string KeybindRowScenePath = "res://Hud/keybind_row.tscn";
    private const string WaitingText = "Press key...";
    private const string WaitingHint = "ESC to cancel. DEL to un-bind.";
    private const string ResetAllText = "Reset Keybinds to Defaults";
    private const string ConfirmResetAllText = "Confirm?";

    private readonly Dictionary<string, Button> keyButtons = [];
    private string? capturing;
    private IReadOnlyList<KeybindRow> shownKeybindRows = [];

    private CheckBox SoundEnabled => GetNode<CheckBox>("Pages/Audio/SoundEnabled");

    private HSlider Volume => GetNode<HSlider>("Pages/Audio/VolumeRow/Volume");

    private HSlider CursorScale => GetNode<HSlider>("Pages/General/Rows/CursorRow/CursorScale");

    private Node KeybindSections => GetNode("Pages/Keybinds/Rows/Sections");

    private HSlider ScreenGamma => GetNode<HSlider>("Pages/Display/ScreenGammaRow/ScreenGamma");

    private HSlider WorldGamma => GetNode<HSlider>("Pages/Display/WorldGammaRow/WorldGamma");

    public event Action<OptionsTab>? TabChosen;

    public event Action? CloseChosen;

    public event Action<bool>? SoundEnabledChanged;

    public event Action<int>? SoundVolumeChanged;

    public event Action<GeneralOption, bool>? OptionChanged;

    public event Action<float>? CursorScaleChanged;

    public event Action<string>? CaptureChosen;

    public event Action<KeyChord>? KeyCaptured;

    public event Action? CaptureCancelled;

    public event Action? CaptureUnbound;

    public event Action<string>? BindingResetChosen;

    public event Action? ResetAllChosen;

    public event Action<float>? ScreenGammaChanged;

    public event Action<float>? WorldGammaChanged;

    public override void _Ready()
    {
        ApplyButtonSprite();
        AddOptionRows();
        ShowKeybinds(KeybindsTab.Sections(new CharacterSettings()), new KeyBindings(), new OptionsWindow());
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
        GetNode<Button>("Pages/Keybinds/Rows/ResetAll").Pressed += () => ResetAllChosen?.Invoke();
        ScreenGamma.ValueChanged += gamma =>
        {
            ShowGammaValue(ScreenGamma, (float)gamma);
            ScreenGammaChanged?.Invoke((float)gamma);
        };
        WorldGamma.ValueChanged += gamma =>
        {
            ShowGammaValue(WorldGamma, (float)gamma);
            WorldGammaChanged?.Invoke((float)gamma);
        };
    }

    // Runs before the chat line and the hotkeys see the key, so the key being bound does nothing else.
    public override void _Input(InputEvent inputEvent)
    {
        if (capturing == null || inputEvent is not InputEventKey key)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        if (!key.Pressed)
        {
            return;
        }

        if (key.Keycode == Key.Escape)
        {
            CaptureCancelled?.Invoke();
        }
        else if (key.Keycode == Key.Delete)
        {
            CaptureUnbound?.Invoke();
        }
        else if (HotkeyInput.Chord(key) is { } chord)
        {
            KeyCaptured?.Invoke(chord);
        }
    }

    public void ShowOptions(OptionsWindow window, ClientSettings? settings, CharacterSettings characterSettings)
    {
        capturing = window.Capturing;
        Visible = window.IsOpen;
        if (!Visible || settings == null)
        {
            return;
        }

        TabButton(window.Tab).SetPressedNoSignal(true);
        GetNode<TabContainer>("Pages").CurrentTab = (int)window.Tab;
        foreach (var row in GeneralOptions.Rows)
        {
            var box = OptionBox(row.Option);
            box.SetPressedNoSignal(settings[row.Option]);
            box.GetParent<Control>().Visible = GeneralOptions.IsShown(row, option => settings[option]);
        }

        CursorScale.SetValueNoSignal(settings.CursorScale);
        ShowCursorScale(settings.CursorScale);
        SoundEnabled.SetPressedNoSignal(settings.SoundEnabled);
        Volume.SetValueNoSignal(settings.SoundVolume);
        ShowVolume(settings.SoundVolume);
        ScreenGamma.SetValueNoSignal(settings.ScreenGamma);
        ShowGammaValue(ScreenGamma, settings.ScreenGamma);
        WorldGamma.SetValueNoSignal(settings.WorldGamma);
        ShowGammaValue(WorldGamma, settings.WorldGamma);
        ShowKeybinds(KeybindsTab.Sections(characterSettings), settings.KeyBindings, window);
    }

    // Buttons draw the game's button sprite, which lives in the sprite archives rather than in the project.
    // Not in the editor: the theme is a shared file there, and saving would write the sprite into it.
    public void ApplyButtonSprite()
    {
        if (Engine.IsEditorHint() || Theme == null
            || HudView.Containing(this)?.Sprites?.GetTexture(ButtonSprite) is not { } texture)
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
            if (row.Under != null)
            {
                Indent(rowNode);
            }

            GetNode(row.GodotOnly ? "Pages/General/Rows/GodotOptions" : "Pages/General/Rows/ClientOptions")
                .AddChild(rowNode);
        }
    }

    private void ShowKeybinds(IReadOnlyList<KeybindSection> sections, KeyBindings bindings, OptionsWindow window)
    {
        var rows = sections.SelectMany(section => section.Rows).ToList();
        if (!rows.SequenceEqual(shownKeybindRows))
        {
            AddKeybindSections(sections);
            shownKeybindRows = rows;
        }

        foreach (var row in rows)
        {
            var waiting = row.Id == window.Capturing;
            var keyButton = keyButtons[row.Id];
            keyButton.Text = waiting ? WaitingText : bindings.LongName(row.Id);
            keyButton.TooltipText = waiting ? WaitingHint : "";
        }

        GetNode<Button>("Pages/Keybinds/Rows/ResetAll").Text =
            window.ConfirmingReset ? ConfirmResetAllText : ResetAllText;
    }

    // Spell rows come and go with the hotbar, so the sections are rebuilt whenever the list of rows changes.
    private void AddKeybindSections(IReadOnlyList<KeybindSection> sections)
    {
        foreach (var oldSection in KeybindSections.GetChildren())
        {
            KeybindSections.RemoveChild(oldSection);
            oldSection.QueueFree();
        }

        keyButtons.Clear();
        var sectionScene = GD.Load<PackedScene>(KeybindSectionScenePath);
        var rowScene = GD.Load<PackedScene>(KeybindRowScenePath);
        foreach (var section in sections)
        {
            var sectionNode = sectionScene.Instantiate<Control>();
            sectionNode.GetNode<CanvasItem>("Separator").Visible = KeybindSections.GetChildCount() > 0;
            sectionNode.GetNode<Label>("Heading").Text = section.Heading;
            foreach (var row in section.Rows)
            {
                var rowNode = rowScene.Instantiate<Control>();
                rowNode.Name = row.Id;
                rowNode.GetNode<Label>("Text").Text = row.Label;
                var keyButton = rowNode.GetNode<Button>("Key");
                keyButton.Pressed += () => CaptureChosen?.Invoke(row.Id);
                keyButtons[row.Id] = keyButton;
                rowNode.GetNode<Button>("Reset").Pressed += () => BindingResetChosen?.Invoke(row.Id);
                sectionNode.GetNode("Rows").AddChild(rowNode);
            }

            KeybindSections.AddChild(sectionNode);
        }
    }

    // The text gives up the indent's width so the row stays inside the page.
    private static void Indent(Control rowNode)
    {
        var indent = rowNode.GetNode<Control>("Indent");
        indent.Visible = true;
        var text = rowNode.GetNode<Control>("Text");
        text.CustomMinimumSize -= new Vector2(indent.CustomMinimumSize.X + rowNode.GetThemeConstant("separation"), 0);
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

    private static void ShowGammaValue(HSlider slider, float gamma) =>
        slider.GetNode<Label>("Value").Text = Gamma.Text(gamma);
}

using System;
using System.Linq;
using Godot;
using TheLastGate.Hud;
using TheLastGate.Notes;
using TheLastGate.Windows;

namespace TheLastGate.Map;

// The current client's World Map window, drawn with the game's window and button sprites: a title bar to drag it by,
// the category column (filled by a later step), the map with the player's notes on it, and a bar of tools, Follow
// player and the tile under the cursor. Its own clicks change its own state; the HUD only opens it and tells it what
// to show.
[Tool]
public partial class WorldMapWindowView : Panel
{
    private const int FrameSprite = 18128;
    private const float TitleBottom = 30;

    private static readonly Vector2 SmallestSize = new(480, 320);
    private NoteSet? notes;

    private Vector2 titleGrab;
    private bool titleHeld;
    private WorldMapWindow? window;

    private WorldMapCanvas Canvas => GetNode<WorldMapCanvas>("Rows/Body/MapFrame/Canvas");

    private Control Help => GetNode<Control>("HelpBlocker");

    private CheckBox ShowOnMinimapBox => GetNode<CheckBox>("Rows/Body/Categories/Footer/ShowOnMinimap");

    public event Action? NotesChanged;

    public override void _Ready()
    {
        ApplyFrameSprite();
        AddHelpLines();
        if (Engine.IsEditorHint())
        {
            return;
        }

        var close = GetNode<Button>("Rows/TitleBar/Close");
        var cross = close.GetNode<PixelCross>("Cross");
        close.MouseEntered += () => cross.Hovered = true;
        close.MouseExited += () => cross.Hovered = false;
        close.Pressed += () =>
        {
            window?.Close();
            Visible = false;
        };
        GetNode<Button>("Rows/TitleBar/Help").Pressed += OpenHelp;
        GetNode<Control>("Rows/TitleBar").GuiInput += DragByTitle;
        GetNode<ResizeGrip>("ResizeGrip").Dragged += Resize;
        GetNode<CheckBox>("Rows/BottomBar/Follow").Toggled += isOn =>
        {
            if (window != null)
            {
                window.Follow = isOn;
            }
        };
        ShowOnMinimapBox.Toggled += ShowOnMinimap;
        foreach (var tool in Enum.GetValues<WorldMapTool>())
        {
            ToolButton(tool).Pressed += () =>
            {
                if (window != null)
                {
                    window.Tool = tool;
                }
            };
        }

        GetNode<Button>("HelpBlocker/Help/Margins/Rows/Buttons/Back").Pressed += () => ChangeHelpPage(-1);
        GetNode<Button>("HelpBlocker/Help/Margins/Rows/Buttons/Next").Pressed += () => ChangeHelpPage(1);
        GetNode<Button>("HelpBlocker/Help/Margins/Rows/Buttons/Close").Pressed += () => Help.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || window == null || !Visible)
        {
            return;
        }

        GetNode<Label>("Rows/BottomBar/Status").Text = window.StatusAt(Time.GetTicksMsec() / 1000.0);
        GetNode<Label>("Rows/BottomBar/Coordinates").Text =
            Canvas.HoveredTile is { } tile ? $"{tile.X}, {tile.Y}" : "";
        GetNode<CheckBox>("Rows/BottomBar/Follow").SetPressedNoSignal(window.Follow);
        ShowOnMinimapBox.SetPressedNoSignal(notes?.ShowOnMinimap == true);
    }

    // The window, the map it draws, the notes and the player's tile come from the HUD on every refresh.
    public void ShowWindow(
        WorldMapWindow shownWindow,
        ExploredMap? map,
        NoteSet? shownNotes,
        (int X, int Y) playerTile)
    {
        window = shownWindow;
        notes = shownNotes;
        ShowOnMinimapBox.Disabled = shownNotes == null;
        Canvas.Notes = shownNotes;
        Visible = shownWindow.IsOpen;
        Canvas.Window = shownWindow;
        Canvas.Map = map;
        Canvas.PlayerTile = playerTile;
        foreach (var tool in Enum.GetValues<WorldMapTool>())
        {
            var button = ToolButton(tool);
            button.Disabled = !WorldMapWindow.IsAvailable(tool, false);
            button.SetPressedNoSignal(shownWindow.Tool == tool);
        }
    }

    // The window and its inner panels are framed by the game's window sprite, which lives in the sprite archives.
    public void ApplyFrameSprite()
    {
        if (HudView.Containing(this)?.Sprites?.GetTexture(FrameSprite) is not { } texture)
        {
            return;
        }

        foreach (var frame in FindChildren("Frame", nameof(NinePatchRect), true, false).Cast<NinePatchRect>())
        {
            frame.Texture = texture;
        }
    }

    private void ShowOnMinimap(bool isOn)
    {
        if (notes == null)
        {
            return;
        }

        notes.ShowOnMinimap = isOn;
        notes.Changed = true;
        NotesChanged?.Invoke();
    }

    private Button ToolButton(WorldMapTool tool) => GetNode<Button>($"Rows/BottomBar/{tool}");

    // The whole title bar stays on screen, so the window can always be grabbed again.
    private void DragByTitle(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left } click:
                titleHeld = click.Pressed;
                titleGrab = GetGlobalMousePosition() - Position;
                break;
            case InputEventMouseMotion when titleHeld:
                var screen = GetViewportRect().Size;
                var wanted = GetGlobalMousePosition() - titleGrab;
                Position = new Vector2(
                    Mathf.Clamp(wanted.X, 0, Mathf.Max(screen.X - Size.X, 0)),
                    Mathf.Clamp(wanted.Y, 0, Mathf.Max(screen.Y - TitleBottom, 0)));
                break;
        }
    }

    private void Resize(Vector2 change) => Size = (Size + change).Max(SmallestSize);

    private void OpenHelp()
    {
        window?.OpenHelp();
        ShowHelpPage();
        Help.Visible = true;
    }

    private void ChangeHelpPage(int step)
    {
        if (step > 0)
        {
            window?.NextHelpPage();
        }
        else
        {
            window?.PreviousHelpPage();
        }

        ShowHelpPage();
    }

    private void ShowHelpPage()
    {
        var pageNumber = window?.HelpPage ?? 0;
        var page = WorldMapHelp.Pages[pageNumber];
        GetNode<Label>("HelpBlocker/Help/Margins/Rows/Page/Title").Text = page.Title;
        var lines = GetNode("HelpBlocker/Help/Margins/Rows/Page/Lines");
        for (var index = 0; index < lines.GetChildCount(); index++)
        {
            var line = lines.GetChild<Control>(index);
            line.Visible = index < page.Lines.Count;
            if (line.Visible)
            {
                line.GetNode<Label>("Text").Text = page.Lines[index];
            }
        }

        GetNode<Label>("HelpBlocker/Help/Margins/Rows/Buttons/PageNumber").Text =
            window?.HelpPageText ?? $"1 / {WorldMapHelp.Pages.Count}";
        GetNode<Button>("HelpBlocker/Help/Margins/Rows/Buttons/Back").Disabled = pageNumber == 0;
        GetNode<Button>("HelpBlocker/Help/Margins/Rows/Buttons/Next").Disabled =
            pageNumber == WorldMapHelp.Pages.Count - 1;
    }

    // One bullet row for each line the longest page has; pages with fewer hide the rest.
    private void AddHelpLines()
    {
        var lines = GetNode("HelpBlocker/Help/Margins/Rows/Page/Lines");
        var rowScene = GD.Load<PackedScene>("res://Map/help_line.tscn");
        var mostLines = WorldMapHelp.Pages.Max(page => page.Lines.Count);
        for (var index = lines.GetChildCount(); index < mostLines; index++)
        {
            lines.AddChild(rowScene.Instantiate());
        }

        ShowHelpPage();
    }
}

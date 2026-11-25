using System;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// Eight waypoints at a time; one not found yet shows the dark picture and no name. The search field's placeholder says
// what a click would do, because the window's art has no room for another line.
public partial class WaypointWindowView : Control
{
    private const int UnknownPicture = 18340;
    private const int StockPicture = 18341;
    private const float HighlightBrightness = 2f;
    private const string UnknownName = "Unknown Location";
    private const string UnknownDescription = "\"???\"";

    // The caret sits at the start of an empty field, on top of the placeholder's first letter.
    private const string HintIndent = "  ";

    // The current client's wheel area, relative to the blocker: the rows, not the scrollbar.
    private static readonly Rect2 WheelArea = new(0, 1, 266, 279);
    private int? hoveredRow;

    private bool wasOpen;
    private WaypointWindow? window;

    public LineEdit Search => GetNode<LineEdit>("Search");

    public ScrollBar ScrollBar => GetNode<ScrollBar>("ScrollBackground/ScrollBar");

    public event Action? SelectedChosen;

    public event Action? CloseAsked;

    public static bool InWheelArea(Vector2 position) => WheelArea.HasPoint(position);

    public Control Row(int row) => GetNode("Blocker/Rows").GetChild<Control>(row);

    public override void _Ready()
    {
        for (var row = 0; row < WaypointWindow.VisibleRows; row++)
        {
            var shownRow = row;
            Row(row).MouseEntered += () => Hover(shownRow);
            Row(row).MouseExited += () => Hover(null);
        }

        Search.TextChanged += SearchFor;
        Search.TextSubmitted += _ => SelectedChosen?.Invoke();
        Search.GuiInput += SearchKey;
    }

    public void ShowWaypoints(WaypointWindow shownWindow)
    {
        window = shownWindow;
        Visible = window.IsOpen;
        FollowOpening(window.IsOpen);
        if (!Visible)
        {
            return;
        }

        for (var row = 0; row < WaypointWindow.VisibleRows; row++)
        {
            var rowNode = Row(row);
            rowNode.Visible = window.HasRow(row);
            if (!rowNode.Visible)
            {
                continue;
            }

            var waypoint = window.At(row);
            var picture = rowNode.GetNode<ArchiveSprite>("Picture");
            picture.SpriteNumber = !waypoint.Found ? UnknownPicture
                : waypoint.Picture != 0 ? waypoint.Picture
                : StockPicture;
            var bright = waypoint.Found && !waypoint.YouAreHere && (window.IsSelected(row) || hoveredRow == row);
            picture.Modulate = bright
                ? new Color(HighlightBrightness, HighlightBrightness, HighlightBrightness)
                : Colors.White;
            var colour = !waypoint.Found ? TextColour.Dim
                : waypoint.YouAreHere ? TextColour.Green
                : bright ? TextColour.Blue
                : TextColour.Yellow;
            var name = rowNode.GetNode<HudLabel>("Name");
            var description = rowNode.GetNode<HudLabel>("Description");
            name.Text = waypoint.Found ? waypoint.Name : UnknownName;
            description.Text = waypoint.Found ? waypoint.Line : UnknownDescription;
            name.Colour = colour;
            description.Colour = colour;
        }

        Search.PlaceholderText = HintIndent + window.Hint(hoveredRow);
        ShowScroll(window);
    }

    // Zeroed first so a shorter list's maximum never clamps the value and sends a scroll of its own.
    private void ShowScroll(WaypointWindow window)
    {
        ScrollBar.SetValueNoSignal(0);
        ScrollBar.MaxValue = window.ShownCount;
        ScrollBar.SetValueNoSignal(window.Scroll);
    }

    private void Redraw()
    {
        if (window != null)
        {
            ShowWaypoints(window);
        }
    }

    private void Hover(int? row)
    {
        hoveredRow = row;
        Redraw();
    }

    private void FollowOpening(bool isOpen)
    {
        if (isOpen == wasOpen)
        {
            return;
        }

        wasOpen = isOpen;
        if (isOpen)
        {
            Search.Clear();
            Search.GrabFocus();
            Search.Edit();
        }
        else if (Search.HasFocus())
        {
            GetViewport().GuiReleaseFocus();
        }
    }

    private void SearchFor(string search)
    {
        window?.SetSearch(search);
        Redraw();
    }

    private void ClearAndType()
    {
        Search.Clear();
        window?.ClearSearch();
        TypeInSearch();
        Redraw();
    }

    private void TypeInSearch()
    {
        Search.GrabFocus();
        Search.Edit();
    }

    private void MoveSelection(int rows)
    {
        window?.MoveSelection(rows);
        Redraw();
    }

    private void ClearSelection()
    {
        window?.ClearSelection();
        Redraw();
    }

    // Keycodes, not ui_up and ui_down: NavigationKeys takes those off on the game screen. Escape clears a search
    // first, and closes the window once the field is empty.
    private void SearchKey(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Pressed: true } key)
        {
            return;
        }

        if (key.Keycode == Key.Down)
        {
            MoveSelection(1);
        }
        else if (key.Keycode == Key.Up && window?.Selected == 0)
        {
            ClearSelection();
        }
        else if (key.Keycode == Key.Up && window?.Selected != null)
        {
            MoveSelection(-1);
        }
        else if (key.Keycode == Key.Escape && Search.Text.Length > 0)
        {
            ClearAndType();
        }
        else if (key.Keycode == Key.Escape)
        {
            CloseAsked?.Invoke();
        }
        else
        {
            return;
        }

        AcceptEvent();
    }
}

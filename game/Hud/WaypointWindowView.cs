using Godot;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// Eight of the 27 waypoints at a time; one not found yet shows its dark picture and no name.
public partial class WaypointWindowView : Control
{
    private const int FoundPicture = 4500;
    private const int UnknownPicture = 4533;
    private const float HighlightBrightness = 2f;
    private const string UnknownName = "Unknown Location";
    private const string UnknownDescription = "\"???\"";

    // The current client's wheel area, relative to the blocker: the rows, not the scrollbar.
    private static readonly Rect2 WheelArea = new(0, 1, 266, 279);

    public static bool InWheelArea(Vector2 position) => WheelArea.HasPoint(position);

    public Control Row(int row) => GetNode("Blocker/Rows").GetChild<Control>(row);

    public override void _Ready()
    {
        for (var row = 0; row < WaypointWindow.VisibleRows; row++)
        {
            var picture = Row(row).GetNode<CanvasItem>("Picture");
            var highlighted = new Color(HighlightBrightness, HighlightBrightness, HighlightBrightness);
            Row(row).MouseEntered += () => picture.Modulate = highlighted;
            Row(row).MouseExited += () => picture.Modulate = Colors.White;
        }
    }

    public void ShowWaypoints(GameState state)
    {
        var window = state.WaypointWindow;
        Visible = window.IsOpen;
        if (!Visible)
        {
            return;
        }

        for (var row = 0; row < WaypointWindow.VisibleRows; row++)
        {
            var waypoint = window.At(row);
            var found = Waypoints.Found(state.Player.Waypoints, waypoint.Number);
            var rowNode = Row(row);
            rowNode.GetNode<ArchiveSprite>("Picture").SpriteNumber =
                (found ? FoundPicture : UnknownPicture) + waypoint.Number;
            rowNode.GetNode<Label>("Name").Text = found ? waypoint.Name : UnknownName;
            rowNode.GetNode<Label>("Description").Text = found ? waypoint.Description : UnknownDescription;
        }

        var bar = GetNode<Control>("Bar");
        bar.Position = bar.Position with { Y = WaypointWindow.BarTop(window.Scroll) };
    }
}

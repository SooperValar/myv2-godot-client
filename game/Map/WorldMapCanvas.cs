using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using TheLastGate.Notes;
using TheLastGate.Windows;
using NumericsVector = System.Numerics.Vector2;

namespace TheLastGate.Map;

// The world map's picture area: the explored map, panned and zoomed, with the player's notes and the player on it.
public partial class WorldMapCanvas : Control
{
    private const int BytesPerPixel = 4;
    private const ulong RefreshMilliseconds = 500;
    private const int MarkerOutline = 2;
    private const int MarkerCentre = 1;
    private const float FollowRingRadius = 5;
    private const int FollowRingPoints = 17;
    private const double PinHoverRadius = 8;
    private const float PathThickness = 2;
    private const float PathEdgeThickness = 4;
    private const float ChevronEdgeThickness = 3;
    private const int PathEndOutline = 3;
    private const int PathEndFill = 2;
    private const int PathStartHole = 1;

    private static readonly Color Gold = Color.Color8(243, 219, 106);
    private static readonly Color PathEdge = Color.Color8(0, 0, 0, 200);
    private static readonly Color TextPlate = Color.Color8(17, 5, 5, 200);

    // Lines are drawn through the middle of their pixels.
    private static readonly Vector2 PixelMiddle = new(0.5f, 0.5f);

    private readonly byte[] pixels = new byte[WorldMapPicture.Width * WorldMapPicture.Height * BytesPerPixel];
    private readonly WorldMapView view = new();
    private bool panning;
    private Image? picture;
    private ulong pictureMadeAt;
    private int pictureRevision = -1;
    private ImageTexture? pictureTexture;
    private IReadOnlyDictionary<int, LabelRect> placedNames = new Dictionary<int, LabelRect>();

    public ExploredMap? Map { get; set; }

    public NoteSet? Notes { get; set; }

    public WorldMapWindow? Window { get; set; }

    public (int X, int Y) PlayerTile { get; set; }

    public (int X, int Y)? HoveredTile { get; private set; }

    private Font TextFont => GetThemeFont("font", "Label");

    private int TextSize => GetThemeFontSize("font_size", "Label");

    public override void _Process(double delta)
    {
        if (Window == null || !IsVisibleInTree())
        {
            return;
        }

        if (Window.CentreOnPlayer)
        {
            view.CentreOn(PlayerTile.X, PlayerTile.Y);
            Window.CentreOnPlayer = false;
        }

        if (Window.Follow)
        {
            view.MoveTo(PlayerTile.X, PlayerTile.Y);
        }

        view.Clamp(Size.X, Size.Y);
        RefreshPicture();
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton
            {
                ButtonIndex: MouseButton.WheelUp or MouseButton.WheelDown, Pressed: true,
            } wheel:
                var notches = wheel.Factor > 0 ? wheel.Factor : 1;
                var steps = wheel.ButtonIndex == MouseButton.WheelUp ? notches : -notches;
                view.ZoomAt(Size.X, Size.Y, wheel.Position.X, wheel.Position.Y, Math.Pow(WorldMapView.ZoomStep, steps));
                AcceptEvent();
                break;
            case InputEventMouseButton click when StartsPanning(click.ButtonIndex):
                panning = click.Pressed;
                AcceptEvent();
                break;
            case InputEventMouseMotion motion:
                if (panning)
                {
                    view.Pan(Size.X, Size.Y, motion.Relative.X, motion.Relative.Y);
                    Window?.Panned();
                }

                HoveredTile = view.TileUnder(Size.X, Size.Y, motion.Position.X, motion.Position.Y);
                break;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationMouseExit)
        {
            HoveredTile = null;
        }
    }

    // A pin's name shows on hover only when it isn't already written beside the pin.
    public override string _GetTooltip(Vector2 atPosition)
    {
        if (Notes == null
            || view.HitTest(Notes, Size.X, Size.Y, atPosition.X, atPosition.Y, PinHoverRadius) is not { } hit
            || Notes.FindNote(hit.NoteId) is not { Kind: NoteKind.Pin } pin
            || pin.Label.Length == 0
            || placedNames.ContainsKey(pin.Id))
        {
            return "";
        }

        return pin.Label;
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Colors.Black);
        DrawWorld();
        DrawNotes();
        DrawPlayer();
    }

    private static Color Colour(uint rgb) => Color.Color8((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

    private static Vector2 ToGodot(NumericsVector point) => new(point.X, point.Y);

    // Right-drag always pans; with the Pan tool so does the left button.
    private bool StartsPanning(MouseButton button) =>
        button == MouseButton.Right || (button == MouseButton.Left && Window?.Tool == WorldMapTool.Pan);

    private Vector2 ToPanel(double tileX, double tileY)
    {
        var (panelX, panelY) = view.TileToPanel(Size.X, Size.Y, tileX, tileY);
        return new Vector2((float)panelX, (float)panelY);
    }

    private Vector2 TileMiddle(int tileX, int tileY) => ToPanel(tileX + 0.5, tileY + 0.5).Floor();

    // Only the part of the world inside the panel is drawn, from the matching part of the picture.
    private void DrawWorld()
    {
        if (pictureTexture == null)
        {
            return;
        }

        var visibleStart = ToPanel(0, 0).Max(Vector2.Zero);
        var visibleEnd = ToPanel(WorldMapPicture.Height, WorldMapPicture.Width).Min(Size);
        if (visibleEnd.X <= visibleStart.X || visibleEnd.Y <= visibleStart.Y)
        {
            return;
        }

        var (startTileX, startTileY) = view.PanelToTile(Size.X, Size.Y, visibleStart.X, visibleStart.Y);
        var (endTileX, endTileY) = view.PanelToTile(Size.X, Size.Y, visibleEnd.X, visibleEnd.Y);
        var source = new Rect2(
            (float)startTileY,
            (float)startTileX,
            (float)(endTileY - startTileY),
            (float)(endTileX - startTileX));
        DrawTextureRectRegion(pictureTexture, new Rect2(visibleStart, visibleEnd - visibleStart), source);
    }

    private void DrawNotes()
    {
        if (Notes == null)
        {
            placedNames = new Dictionary<int, LabelRect>();
            return;
        }

        var font = TextFont;
        var fontSize = TextSize;
        placedNames = WorldMapLabels.Place(Notes, view, Size.X, Size.Y, Measure);
        foreach (var category in Notes.Categories)
        {
            if (!Notes.IsShown(category))
            {
                continue;
            }

            foreach (var note in category.Notes)
            {
                var colour = Colour(NoteSet.Rgb(category, note));
                switch (note.Kind)
                {
                    case NoteKind.Path:
                        DrawPath(note, colour);
                        break;
                    case NoteKind.Pin:
                        DrawPin(note, colour);
                        break;
                    case NoteKind.Text:
                        DrawTextLabel(note, colour);
                        break;
                    default:
                        throw new UnreachableException();
                }
            }
        }

        return;

        (float Width, float Height) Measure(string text)
        {
            var size = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
            return (size.X, size.Y);
        }
    }

    private void DrawPin(Note pin, Color colour)
    {
        var (centreX, centreY) = WorldMapLabels.PinCentre(view, Size.X, Size.Y, pin);
        var centre = new Vector2(centreX, centreY);
        DrawDiamond(centre, WorldMapLabels.PinMarkerExtent, Colors.Black);
        DrawDiamond(centre, WorldMapLabels.PinRadius, colour);
        if (placedNames.TryGetValue(pin.Id, out var name))
        {
            DrawShadowedText(new Vector2(name.Left, name.Top), pin.Label, colour);
        }
    }

    private void DrawTextLabel(Note text, Color colour)
    {
        var textSize = TextFont.GetStringSize(text.Label, HorizontalAlignment.Left, -1, TextSize);
        var (plate, textX, textY) = WorldMapLabels.TextPlate(view, Size.X, Size.Y, text, (textSize.X, textSize.Y));
        DrawRect(new Rect2(plate.Left, plate.Top, plate.Right - plate.Left, plate.Bottom - plate.Top), TextPlate);
        DrawShadowedText(new Vector2(textX, textY), text.Label, colour);
    }

    // Every segment's dark edge goes down before any of its colour, so the joins between segments stay clean. The
    // start is a hollow square and the end a solid one, with arrowheads between showing the way the path was drawn.
    private void DrawPath(Note path, Color colour)
    {
        if (path.Segments.Count == 0)
        {
            return;
        }

        foreach (var (start, end) in VisibleSegments(path))
        {
            DrawLine(start + PixelMiddle, end + PixelMiddle, PathEdge, PathEdgeThickness, true);
        }

        foreach (var (start, end) in VisibleSegments(path))
        {
            DrawLine(start + PixelMiddle, end + PixelMiddle, colour, PathThickness, true);
            foreach (var (first, tip, last) in PathMarks.Chevrons(
                new NumericsVector(start.X, start.Y),
                new NumericsVector(end.X, end.Y)))
            {
                Vector2[] points = [ToGodot(first), ToGodot(tip), ToGodot(last)];
                DrawPolyline(points, PathEdge, ChevronEdgeThickness, true);
                DrawPolyline(points, colour, 1, true);
            }
        }

        var firstSegment = path.Segments[0];
        var lastSegment = path.Segments[^1];
        var startMark = TileMiddle(firstSegment.StartX, firstSegment.StartY);
        var endMark = TileMiddle(lastSegment.EndX, lastSegment.EndY);
        DrawSquare(startMark, PathEndOutline, PathEdge);
        DrawSquare(startMark, PathEndFill, colour);
        DrawSquare(startMark, PathStartHole, Colors.Black);
        DrawSquare(endMark, PathEndOutline, PathEdge);
        DrawSquare(endMark, PathEndFill, colour);
    }

    private IEnumerable<(Vector2 Start, Vector2 End)> VisibleSegments(Note path)
    {
        const float margin = PathThickness + 2;
        foreach (var segment in path.Segments)
        {
            var start = TileMiddle(segment.StartX, segment.StartY);
            var end = TileMiddle(segment.EndX, segment.EndY);
            var missesPanel = Mathf.Max(start.X, end.X) < -margin
                || Mathf.Min(start.X, end.X) > Size.X + margin
                || Mathf.Max(start.Y, end.Y) < -margin
                || Mathf.Min(start.Y, end.Y) > Size.Y + margin;
            if (!missesPanel)
            {
                yield return (start, end);
            }
        }
    }

    private void DrawShadowedText(Vector2 topLeft, string text, Color colour)
    {
        var baseline = topLeft + new Vector2(0, TextFont.GetAscent(TextSize));
        DrawString(TextFont, baseline + Vector2.One, text, HorizontalAlignment.Left, -1, TextSize, Colors.Black);
        DrawString(TextFont, baseline, text, HorizontalAlignment.Left, -1, TextSize, colour);
    }

    private void DrawDiamond(Vector2 centre, int radius, Color colour)
    {
        for (var row = -radius; row <= radius; row++)
        {
            var halfWidth = radius - Math.Abs(row);
            DrawRect(new Rect2(centre.X - halfWidth, centre.Y + row, halfWidth * 2 + 1, 1), colour);
        }
    }

    private void DrawPlayer()
    {
        var centre = TileMiddle(PlayerTile.X, PlayerTile.Y);
        if (Window?.Follow == true)
        {
            DrawArc(centre + PixelMiddle, FollowRingRadius, 0, Mathf.Tau, FollowRingPoints, Gold, 1);
        }

        DrawSquare(centre, MarkerOutline, Colors.Black);
        DrawSquare(centre, MarkerCentre, Colors.White);
    }

    private void DrawSquare(Vector2 centre, int radius, Color colour) =>
        DrawRect(new Rect2(centre - new Vector2(radius, radius), new Vector2(radius * 2 + 1, radius * 2 + 1)), colour);

    // Remade at most twice a second while the map changes, as the current client does.
    private void RefreshPicture()
    {
        if (Map == null || Map.Revision == pictureRevision
            || (pictureTexture != null && Time.GetTicksMsec() - pictureMadeAt < RefreshMilliseconds))
        {
            return;
        }

        WorldMapPicture.Render(Map, pixels);
        pictureRevision = Map.Revision;
        pictureMadeAt = Time.GetTicksMsec();
        if (picture == null || pictureTexture == null)
        {
            picture = Image.CreateFromData(
                WorldMapPicture.Width,
                WorldMapPicture.Height,
                false,
                Image.Format.Rgba8,
                pixels);
            pictureTexture = ImageTexture.CreateFromImage(picture);
            return;
        }

        picture.SetData(WorldMapPicture.Width, WorldMapPicture.Height, false, Image.Format.Rgba8, pixels);
        pictureTexture.Update(picture);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using TheLastGate.Notes;
using TheLastGate.Windows;
using NumericsVector = System.Numerics.Vector2;

namespace TheLastGate.Map;

// The world map's picture area: the explored map, panned and zoomed, with the player's notes and the player on it.
// Clicks and drags go to the window's editor; a note being edited is drawn as the edit has left it.
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
    private const int PinSelectionRadius = 6;
    private const float SelectedPathThickness = PathEdgeThickness + 2;

    private static readonly Color Gold = Color.Color8(243, 219, 106);
    private static readonly Color PathEdge = Color.Color8(0, 0, 0, 200);
    private static readonly Color TextPlate = Color.Color8(17, 5, 5, 200);

    // Lines are drawn through the middle of their pixels.
    private static readonly Vector2 PixelMiddle = new(0.5f, 0.5f);

    private readonly byte[] pixels = new byte[WorldMapPicture.Width * WorldMapPicture.Height * BytesPerPixel];
    private readonly WorldMapView view = new();

    private Vector2 mouse;

    // Each shown name relative to its pin, kept while panning; laid out again when what it depends on changes.
    private IReadOnlyDictionary<int, LabelRect> nameOffsets = new Dictionary<int, LabelRect>();

    private (NoteSet Notes, int Revision, double PixelsPerTile, int Selected, Note? Edited, Font Font, int Size)?
        namesFor;

    private bool panning;
    private Image? picture;
    private ulong pictureMadeAt;
    private int pictureRevision = -1;
    private ImageTexture? pictureTexture;

    public ExploredMap? Map { get; set; }

    public NoteSet? Notes { get; set; }

    public WorldMapWindow? Window { get; set; }

    public (int X, int Y) PlayerTile { get; set; }

    public (int X, int Y)? HoveredTile { get; private set; }

    private WorldMapEditor? Editor => Window?.Editor;

    private WorldMapPanel Panel => new(view, Size.X, Size.Y);

    private Font TextFont => GetThemeFont("font", "Label");

    private int TextSize => GetThemeFontSize("font_size", "Label");

    public event Action<NoteHit, Vector2>? NoteMenuWanted;

    public event Action? NotesEdited;

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

        if (Window.MoveRequest is { } target)
        {
            view.MoveTo(target.X, target.Y);
            Window.MoveRequest = null;
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
            case InputEventMouseButton { ButtonIndex: MouseButton.Left or MouseButton.Right } click:
                if (StartsPanning(click.ButtonIndex))
                {
                    panning = click.Pressed;
                }

                Click(click);
                AcceptEvent();
                break;
            case InputEventMouseMotion motion:
                if (panning)
                {
                    view.Pan(Size.X, Size.Y, motion.Relative.X, motion.Relative.Y);
                    Window?.Panned();
                }

                mouse = motion.Position;
                Editor?.MoveMouse(Panel, mouse.X, mouse.Y);
                HoveredTile = view.TileUnder(Size.X, Size.Y, mouse.X, mouse.Y);
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
            || nameOffsets.ContainsKey(pin.Id))
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
        DrawPendingPath();
        DrawPreview();
        DrawPlayer();
    }

    private static Color Colour(uint rgb) => Color.Color8((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

    private static Vector2 ToGodot(NumericsVector point) => new(point.X, point.Y);

    private static Color Colour(NoteSet notes, int categoryId) =>
        notes.FindCategory(categoryId) is { } category ? Colour(NotePalette.Rgb(category.ColourIndex)) : Colors.White;

    // Right-drag always pans; with the Pan tool so does the left button.
    private bool StartsPanning(MouseButton button) =>
        button == MouseButton.Right || (button == MouseButton.Left && Window?.Tool == WorldMapTool.Pan);

    private Vector2 ToPanel(double tileX, double tileY)
    {
        var (panelX, panelY) = view.TileToPanel(Size.X, Size.Y, tileX, tileY);
        return new Vector2((float)panelX, (float)panelY);
    }

    private Vector2 TileMiddle(int tileX, int tileY) => ToPanel(tileX + 0.5, tileY + 0.5).Floor();

    // A right click on a note, rather than a right drag, asks for the note's menu where the mouse is.
    private void Click(InputEventMouseButton click)
    {
        if (Notes == null || Editor == null)
        {
            return;
        }

        var (pointX, pointY) = (click.Position.X, click.Position.Y);
        switch (click)
        {
            case { ButtonIndex: MouseButton.Left, Pressed: true }:
                Editor.PressLeft(Panel, pointX, pointY);
                break;
            case { ButtonIndex: MouseButton.Left }:
                Editor.ReleaseLeft(Notes, Panel, pointX, pointY);
                NotesEdited?.Invoke();
                break;
            case { Pressed: true }:
                Editor.PressRight(pointX, pointY);
                break;
            default:
                if (Editor.ReleaseRight(Notes, Panel, pointX, pointY) is { } hit)
                {
                    NoteMenuWanted?.Invoke(hit, GetViewport().GetMousePosition());
                }

                break;
        }
    }

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
            nameOffsets = new Dictionary<int, LabelRect>();
            namesFor = null;
            return;
        }

        var font = TextFont;
        var fontSize = TextSize;
        var selectedId = Editor?.SelectedNoteId ?? 0;
        var edited = Editor?.Edited;
        var namesNeeded = (Notes, Notes.Revision, view.PixelsPerTile, selectedId, edited, font, fontSize);
        if (namesFor != namesNeeded)
        {
            nameOffsets = WorldMapLabels.Layout(Notes, view.PixelsPerTile, Measure, selectedId, edited);
            namesFor = namesNeeded;
        }

        foreach (var category in Notes.Categories)
        {
            if (!Notes.IsShown(category))
            {
                continue;
            }

            foreach (var stored in category.Notes)
            {
                var note = edited?.Id == stored.Id ? edited : stored;
                var colour = Colour(NoteSet.Rgb(category, note));
                var selected = note.Id == selectedId;
                switch (note.Kind)
                {
                    case NoteKind.Path:
                        DrawPath(note, colour, selected);
                        break;
                    case NoteKind.Pin:
                        DrawPin(note, colour, selected);
                        break;
                    case NoteKind.Text:
                        DrawTextLabel(note, colour, selected);
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

    private void DrawPin(Note pin, Color colour, bool selected)
    {
        var (centreX, centreY) = WorldMapLabels.PinCentre(view, Size.X, Size.Y, pin);
        var centre = new Vector2(centreX, centreY);
        if (selected)
        {
            DrawDottedDiamond(centre, PinSelectionRadius);
        }

        DrawDiamond(centre, WorldMapLabels.PinMarkerExtent, Colors.Black);
        DrawDiamond(centre, WorldMapLabels.PinRadius, colour);
        if (nameOffsets.TryGetValue(pin.Id, out var name))
        {
            DrawShadowedText(centre + new Vector2(name.Left, name.Top), pin.Label, colour);
        }
    }

    private void DrawTextLabel(Note text, Color colour, bool selected)
    {
        var textSize = TextFont.GetStringSize(text.Label, HorizontalAlignment.Left, -1, TextSize);
        var (plate, textX, textY) = WorldMapLabels.TextPlate(view, Size.X, Size.Y, text, (textSize.X, textSize.Y));
        DrawRect(new Rect2(plate.Left, plate.Top, plate.Right - plate.Left, plate.Bottom - plate.Top), TextPlate);
        if (selected)
        {
            DrawDottedRect(plate);
        }

        DrawShadowedText(new Vector2(textX, textY), text.Label, colour);
    }

    // Every segment's dark edge goes down before any of its colour, so the joins between segments stay clean. The
    // start is a hollow square and the end a solid one, with arrowheads between showing the way the path was drawn. A
    // selected path has a dashed gold border under its edge.
    private void DrawPath(Note path, Color colour, bool selected)
    {
        if (path.Segments.Count == 0)
        {
            return;
        }

        if (selected)
        {
            foreach (var (start, end) in VisibleSegments(path))
            {
                DrawDashedLine(start + PixelMiddle, end + PixelMiddle, Gold, SelectedPathThickness);
            }
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
        foreach (var segment in path.Segments)
        {
            var start = TileMiddle(segment.StartX, segment.StartY);
            var end = TileMiddle(segment.EndX, segment.EndY);
            if (!MissesPanel(start, end))
            {
                yield return (start, end);
            }
        }
    }

    private bool MissesPanel(Vector2 start, Vector2 end)
    {
        const float margin = SelectedPathThickness;
        return Mathf.Max(start.X, end.X) < -margin
            || Mathf.Min(start.X, end.X) > Size.X + margin
            || Mathf.Max(start.Y, end.Y) < -margin
            || Mathf.Min(start.Y, end.Y) > Size.Y + margin;
    }

    // The path being drawn is dashed in its category's colour until it is named.
    private void DrawPendingPath()
    {
        if (Notes == null || Editor == null)
        {
            return;
        }

        var colour = Colour(Notes, Editor.PendingCategoryId);
        foreach (var segment in Editor.PendingSegments)
        {
            DrawDashedSegment(segment, colour);
        }
    }

    private void DrawPreview()
    {
        if (Notes != null && Editor?.Preview(Panel, mouse.X, mouse.Y) is { } segment)
        {
            DrawDashedSegment(segment, Colour(Notes, Editor.DrawingCategoryId(Notes)));
        }
    }

    private void DrawDashedSegment(PathSegment segment, Color colour)
    {
        var start = ToPanel(segment.StartX + 0.5, segment.StartY + 0.5);
        var end = ToPanel(segment.EndX + 0.5, segment.EndY + 0.5);
        if (!MissesPanel(start, end))
        {
            DrawDashedLine(start, end, colour, PathThickness);
        }
    }

    private void DrawDashedLine(Vector2 start, Vector2 end, Color colour, float thickness)
    {
        foreach (var (dashStart, dashEnd) in PathMarks.Dashes(
            new NumericsVector(start.X, start.Y),
            new NumericsVector(end.X, end.Y)))
        {
            DrawLine(ToGodot(dashStart), ToGodot(dashEnd), colour, thickness, true);
        }
    }

    // Every other pixel round a diamond or a rectangle, in gold.
    private void DrawDottedDiamond(Vector2 centre, int radius)
    {
        for (var row = -radius; row <= radius; row += 2)
        {
            var halfWidth = radius - Math.Abs(row);
            DrawRect(new Rect2(centre.X - halfWidth, centre.Y + row, 1, 1), Gold);
            DrawRect(new Rect2(centre.X + halfWidth, centre.Y + row, 1, 1), Gold);
        }
    }

    private void DrawDottedRect(LabelRect rect)
    {
        var width = (int)Math.Ceiling(rect.Right - rect.Left);
        var height = (int)Math.Ceiling(rect.Bottom - rect.Top);
        for (var column = 0; column < width; column += 2)
        {
            DrawRect(new Rect2(rect.Left + column, rect.Top, 1, 1), Gold);
            DrawRect(new Rect2(rect.Left + column, rect.Bottom - 1, 1, 1), Gold);
        }

        for (var row = 0; row < height; row += 2)
        {
            DrawRect(new Rect2(rect.Left, rect.Top + row, 1, 1), Gold);
            DrawRect(new Rect2(rect.Right - 1, rect.Top + row, 1, 1), Gold);
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

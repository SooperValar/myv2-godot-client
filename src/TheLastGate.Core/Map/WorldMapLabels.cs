using System;
using System.Collections.Generic;
using TheLastGate.Notes;

namespace TheLastGate.Map;

// Where pins and text labels sit on the world map, in panel pixels. A pin is a diamond whose name goes in the first
// free spot beside it; a text label is its text centred on its tile on a dark plate.
public static class WorldMapLabels
{
    public const int PinRadius = 3;
    public const int PinMarkerExtent = PinRadius + 1;

    private const double NamesHiddenBelowPixelsPerTile = 2.68;
    private const float NameGap = 4;
    private const float NearPanelMargin = 256;
    private const float TextPaddingX = 4;
    private const float TextPaddingY = 2;

    public static bool NamesHidden(WorldMapView view) => view.PixelsPerTile < NamesHiddenBelowPixelsPerTile;

    public static (float X, float Y) PinCentre(WorldMapView view, double panelWidth, double panelHeight, Note pin)
    {
        var (panelX, panelY) = view.TileToPanel(panelWidth, panelHeight, pin.TileX + 0.5, pin.TileY + 0.5);
        return ((float)Math.Floor(panelX), (float)Math.Floor(panelY));
    }

    public static (LabelRect Plate, float TextX, float TextY) TextPlate(
        WorldMapView view,
        double panelWidth,
        double panelHeight,
        Note text,
        (float Width, float Height) textSize)
    {
        var (anchorX, anchorY) = view.TileToPanel(panelWidth, panelHeight, text.TileX + 0.5, text.TileY + 0.5);
        var textX = MathF.Floor((float)anchorX - textSize.Width / 2);
        var textY = MathF.Floor((float)anchorY - textSize.Height / 2);
        var plate = new LabelRect(
            textX - TextPaddingX,
            textY - TextPaddingY,
            textX + MathF.Floor(textSize.Width) + TextPaddingX,
            textY + MathF.Floor(textSize.Height) + TextPaddingY);
        return (plate, textX, textY);
    }

    // Every marker and text plate is reserved before any name is placed, so no name covers one, and the selected pin's
    // name goes first so it gets the best spot. A name with no room is left out; hovering the pin shows it instead.
    public static IReadOnlyDictionary<int, LabelRect> Place(
        NoteSet notes,
        WorldMapView view,
        double panelWidth,
        double panelHeight,
        Func<string, (float Width, float Height)> measure,
        int selectedNoteId = 0,
        Note? edited = null)
    {
        var placed = new Dictionary<int, LabelRect>();
        if (NamesHidden(view))
        {
            return placed;
        }

        var layout = new LabelLayout();
        var named = new List<(Note Pin, (float X, float Y) Centre)>();
        foreach (var category in notes.Categories)
        {
            if (!notes.IsShown(category))
            {
                continue;
            }

            var namesShown = notes.LabelsShown(category);
            foreach (var note in category.Notes)
            {
                if (note.Kind == NoteKind.Text)
                {
                    var (plate, _, _) = TextPlate(view, panelWidth, panelHeight, note, measure(note.Label));
                    if (IsNearPanel(plate.Left, plate.Top))
                    {
                        layout.Reserve(plate);
                    }

                    continue;
                }

                if (note.Kind != NoteKind.Pin)
                {
                    continue;
                }

                var centre = PinCentre(view, panelWidth, panelHeight, note);
                if (!IsNearPanel(centre.X, centre.Y))
                {
                    continue;
                }

                layout.Reserve(MarkerRect(centre));
                if (namesShown && note.Label.Length > 0)
                {
                    named.Add((note, centre));
                }
            }
        }

        var selected = named.FindIndex(entry => entry.Pin.Id == selectedNoteId);
        if (selected >= 0)
        {
            PlaceName(named[selected]);
        }

        for (var index = 0; index < named.Count; index++)
        {
            if (index != selected)
            {
                PlaceName(named[index]);
            }
        }

        return placed;

        bool IsNearPanel(float panelX, float panelY) =>
            panelX >= -NearPanelMargin && panelX <= panelWidth + NearPanelMargin
            && panelY >= -NearPanelMargin && panelY <= panelHeight + NearPanelMargin;

        // The text's shadow sits one pixel right and below, so it is part of the name's footprint.
        void PlaceName((Note Pin, (float X, float Y) Centre) entry)
        {
            var (width, height) = measure(entry.Pin.Label);
            var spot = layout.PlaceBeside(
                entry.Centre.X,
                entry.Centre.Y,
                PinMarkerExtent + NameGap,
                MathF.Floor(width) + 1,
                MathF.Floor(height) + 1);
            if (spot is { } rect)
            {
                placed[entry.Pin.Id] = rect;
            }
        }
    }

    // The outline reaches one pixel further right and down, where the centre pixel itself sits.
    private static LabelRect MarkerRect((float X, float Y) centre) =>
        new(
            centre.X - PinMarkerExtent,
            centre.Y - PinMarkerExtent,
            centre.X + PinMarkerExtent + 1,
            centre.Y + PinMarkerExtent + 1);
}

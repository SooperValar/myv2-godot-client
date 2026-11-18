using System;
using System.Collections.Generic;
using TheLastGate.Notes;

namespace TheLastGate.Map;

public static class WorldMapLabels
{
    public static bool NamesHidden(WorldMapView view) => throw new NotImplementedException();

    public static (float X, float Y) PinCentre(WorldMapView view, double panelWidth, double panelHeight, Note pin) =>
        throw new NotImplementedException();

    public static (LabelRect Plate, float TextX, float TextY) TextPlate(
        WorldMapView view,
        double panelWidth,
        double panelHeight,
        Note text,
        (float Width, float Height) textSize) =>
        throw new NotImplementedException();

    public static IReadOnlyDictionary<int, LabelRect> Place(
        NoteSet notes,
        WorldMapView view,
        double panelWidth,
        double panelHeight,
        Func<string, (float Width, float Height)> measure,
        int selectedNoteId = 0) =>
        throw new NotImplementedException();
}
